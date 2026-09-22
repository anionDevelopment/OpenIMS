import { Component, ChangeDetectionStrategy } from '@angular/core';
import { UserDataService } from '../../../services/user-data.service';

@Component({
  selector: 'app-user-dashboard',
  standalone: false,
  templateUrl: './user-dashboard.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './user-dashboard.component.scss'
})
export class UserDashboardComponent {
  userName: string | null = null;
  constructor(userDataService: UserDataService) {
    userDataService.getUserName().subscribe(userName => {
      this.userName = userName;
    });
  }
}
