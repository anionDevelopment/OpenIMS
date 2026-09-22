import { Component, ChangeDetectionStrategy } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-admin-area-container',
  standalone: false,
  templateUrl: './admin-area-container.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './admin-area-container.component.scss'
})
export class AdminAreaContainerComponent {

  activeSite: string;

  constructor(private router: Router) {
    this.activeSite = this.getSiteTitle(this.router.url.split('/').pop()!);
  }

  onDashboardClick() {
    this.router.navigate(['admin', 'dashboard']);
  }

  onUsersClick() {
    this.router.navigate(['admin', 'users']);
  }

  onSettingsClick() {
    this.router.navigate(['admin', 'settings']);
  }

  onUserAreaClick() {
    this.router.navigate(['user', 'dashboard']);
  }

  getSiteTitle(urlSegment: string): string {
    switch (urlSegment) {
      case "admin": {
        return "Admin-Area";
      }
      case "dashboard": {
        return "Dashboard";
      }
      case "users": {
        return "Users";
      }
      case "settings": {
        return "Settings";
      }
      case "user": {
        return "User-Area";
      }
      default: {
        throw new Error('Unknown urlSegment: ' + urlSegment);
      }
    }
  }
}
