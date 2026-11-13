import { Component, ChangeDetectionStrategy } from '@angular/core';
import { FormControl, FormGroup } from '@angular/forms';
import { AccessToken, UserService } from '../../../generated/open-ims-backend';
import { Router } from '@angular/router';
import { UserDataService } from '../../../services/user-data.service';
import { StorageService } from '../../../services/storage.service';
import { switchMap } from 'rxjs';

/**
 * The login-mask. It exchanges the credentials of the user for an access-token and stores that token, which is
 * what every following request of the application uses to authenticate itself.
 */
@Component({
  selector: 'app-login-form',
  standalone: false,
  templateUrl: './login-form.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './login-form.component.scss'
})
export class LoginFormComponent {

  /** Shown when the backend rejected the credentials. It is deliberately unspecific, like the answer of the backend. */
  loginFailed = false;

  constructor(
    private userService: UserService,
    private userDataService: UserDataService,
    private router: Router,
    private storageService: StorageService,
  ) { }

  form: FormGroup = new FormGroup({
    username: new FormControl(''),
    password: new FormControl(''),
  });

  public login(): void {
    const username: string = this.form.get('username')!.value;
    const password: string = this.form.get('password')!.value;
    this.loginFailed = false;
    this.userService.aPIV1UserControllerLoginPut(username, password)
      .pipe(
        switchMap((accessToken: AccessToken) => {
          this.storageService.setAccessToken(accessToken.value!);
          return this.userDataService.loadUserData();
        })
      ).subscribe({
        next: () => {
          this.router.navigate(['user', 'dashboard']);
        },
        error: () => {
          // The backend answers an unknown user and a wrong password identically, so the message here must not
          // distinguish the two cases either.
          this.loginFailed = true;
          this.storageService.setAccessToken(null);
        }
      });
  }
}
