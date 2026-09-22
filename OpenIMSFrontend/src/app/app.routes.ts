import { Routes } from '@angular/router';
import { HomePageComponent } from './modules/home-page/home-page/home-page.component';
import { UserDashboardComponent } from './modules/user-area/user-dashboard/user-dashboard.component';
import { AdminDashboardComponent } from './modules/admin-area/admin-dashboard/admin-dashboard.component';
import { authenticationCheckGuard } from './guards/authentication-check.guard';
import { UserSettingsComponent } from './modules/user-area/user-settings/user-settings.component';
import { UsersListComponent } from './modules/admin-area/users-list/users-list.component';
import { UserComponent } from './modules/admin-area/user/user.component';
import { AdminSettingsComponent } from './modules/admin-area/admin-settings/admin-settings.component';
import { HintNotFoundComponent } from './modules/home-page/hint-not-found/hint-not-found.component';

export const routes: Routes = [
    { path: '', component: HomePageComponent },
    { path: 'admin/dashboard', component: AdminDashboardComponent, canActivate: [authenticationCheckGuard] },
    { path: 'admin/settings', component: AdminSettingsComponent, canActivate: [authenticationCheckGuard] },
    { path: 'admin/users', component: UsersListComponent, canActivate: [authenticationCheckGuard] },
    { path: 'admin/user', component: UserComponent, canActivate: [authenticationCheckGuard] },
    { path: 'user/dashboard', component: UserDashboardComponent, canActivate: [authenticationCheckGuard] },
    { path: 'user/settings', component: UserSettingsComponent, canActivate: [authenticationCheckGuard] },
    { path: '**', component: HintNotFoundComponent },
];
