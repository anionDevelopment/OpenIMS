import { Component, ChangeDetectionStrategy } from '@angular/core';
import { UserDataService } from '../../../services/user-data.service';
import { UserService } from '../../../generated/open-ims-backend';
import { StorageService } from '../../../services/storage.service';
import { Router } from '@angular/router';

@Component({
  selector: 'app-user-icon',
  standalone: false,
  templateUrl: './user-icon.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './user-icon.component.scss'
})
export class UserIconComponent {
  userId: string | null = null;
  userName: string | null = null;
  constructor(private userDataService: UserDataService, private storageService: StorageService, private router: Router, private userService: UserService) {
    userDataService.getUserId().subscribe(userId => {
      this.userId = userId;
    });
    userDataService.getUserName().subscribe(userName => {
      this.userName = userName;
    });
  }
  logout() {
    this.userService.aPIV1UserControllerLogoutPut(this.storageService.getAccessToken()).subscribe(() => {
      this.userDataService.unloadUserData();
      this.router.navigate(['']);
    });
  }
}
