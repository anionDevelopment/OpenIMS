import { Component, ChangeDetectionStrategy } from '@angular/core';
import { Router } from '@angular/router';
import { UserDataService } from '../../../services/user-data.service';
import { UtilitiesService } from '../../../services/utilities.service';

/**
 * The frame of every page of the user-area: the menu on the left, the title and the user-icon on top.
 */
@Component({
  selector: 'app-user-area-container',
  standalone: false,
  templateUrl: './user-area-container.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './user-area-container.component.scss'
})
export class UserAreaContainerComponent {
  activeSite: string;

  /** Whether the current user may enter the admin-area. It is null as long as that is not known yet. */
  userIsAdmin: boolean | null = null;

  constructor(private router: Router, userDataService: UserDataService, utilitiesService: UtilitiesService) {
    this.activeSite = this.getSiteTitle(utilitiesService.getURLWithoutQueryParameter(this.router).split('/')[2]);
    userDataService.userIsAdmin().subscribe((isAdmin) => {
      this.userIsAdmin = isAdmin;
    });
  }

  onDashboardClick() {
    this.router.navigate(['user', 'dashboard']);
  }

  onSettingsClick() {
    this.router.navigate(['user', 'settings']);
  }

  onAdminAreaClick() {
    this.router.navigate(['admin', 'dashboard']);
  }

  getSiteTitle(urlSegment: string): string {
    switch (urlSegment) {
      case "admin": {
        return "Admin-Area";
      }
      case "dashboard": {
        return "Dashboard";
      }
      case "settings": {
        return "Settings";
      }
      default: {
        throw new Error('Unknown urlSegment: "' + urlSegment + '"');
      }
    }
  }
}
