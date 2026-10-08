import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AdminDashboardComponent } from './admin-dashboard/admin-dashboard.component';
import { AdminSettingsComponent } from './admin-settings/admin-settings.component';
import { UserComponent } from './user/user.component';
import { UsersListComponent } from './users-list/users-list.component';
import { GroupsListComponent } from './groups-list/groups-list.component';
import { AdminAreaContainerComponent } from './admin-area-container/admin-area-container.component';
import { HomePageModule } from '../home-page/home-page.module';
import { UserAreaModule } from '../user-area/user-area.module';
import { MatDividerModule } from '@angular/material/divider';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { FormsModule } from '@angular/forms';
import { SharedComponentsModule } from '../shared-components/shared-components.module';

@NgModule({
  declarations: [
    AdminAreaContainerComponent,
    AdminDashboardComponent,
    AdminSettingsComponent,
    UserComponent,
    UsersListComponent,
    GroupsListComponent,
  ],
  imports: [
    CommonModule,
    MatDividerModule,
    MatMenuModule,
    MatTooltipModule,
    MatIconModule,
    MatSidenavModule,
    MatButtonModule,
    MatTabsModule,
    MatSlideToggleModule,
    MatFormFieldModule,
    MatInputModule,
    FormsModule,
    SharedComponentsModule,
    HomePageModule,
    UserAreaModule,
  ]
})
export class AdminAreaModule { }
