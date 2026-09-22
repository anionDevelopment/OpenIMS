import { Component, ChangeDetectionStrategy } from '@angular/core';
import { UserDataService } from '../../../services/user-data.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-home-page',
  standalone: false,
  templateUrl: './home-page.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './home-page.component.scss'
})
export class HomePageComponent {
  constructor(userDataService: UserDataService, router: Router) {
    // A user who is already logged in does not have to log in again, so the home-page forwards to the area of
    // the user instead of showing the login-mask a second time.
    userDataService.userIsLoggedIn().subscribe((isLoggedIn) => {
      if (isLoggedIn) {
        router.navigate(['user', 'dashboard']);
      }
    });
  }
}
