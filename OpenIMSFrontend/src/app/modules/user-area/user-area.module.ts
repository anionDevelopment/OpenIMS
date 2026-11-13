import { NgxCultureSelectorComponent } from '@aniondev/ngx-culture-selector';
import { NgxDarkmodeToggleButtonComponent } from '@aniondev/ngx-darkmode-toggle-button';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TimestampPipe } from '../../pipes/timestamp.pipe';
import { UserDashboardComponent } from './user-dashboard/user-dashboard.component';
import { UserSettingsComponent } from './user-settings/user-settings.component';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';
import { UserAreaContainerComponent } from './user-area-container/user-area-container.component';
import { MatDividerModule } from '@angular/material/divider';
import { HomePageModule } from '../home-page/home-page.module';
import { MatIconModule } from '@angular/material/icon';
import { UserIconComponent } from './user-icon/user-icon.component';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatMenuModule } from '@angular/material/menu';
import { MatTableModule } from '@angular/material/table';
import { MatDialogModule } from '@angular/material/dialog';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSelectModule } from '@angular/material/select';
import { MatTreeModule } from '@angular/material/tree';
import { InventoryComponent } from './inventory/inventory.component';
import { InventoryObjectDialogComponent } from './inventory-object-dialog/inventory-object-dialog.component';

@NgModule({
  declarations: [
    TimestampPipe,
    UserIconComponent,
    UserAreaContainerComponent,
    UserDashboardComponent,
    UserSettingsComponent,
    InventoryComponent,
    InventoryObjectDialogComponent,
  ],
  imports: [
    CommonModule,
    NgxDarkmodeToggleButtonComponent,
    NgxCultureSelectorComponent,
    ReactiveFormsModule,
    FormsModule,
    MatFormFieldModule,
    MatDividerModule,
    MatCheckboxModule,
    MatSelectModule,
    MatMenuModule,
    MatTooltipModule,
    MatInputModule,
    MatDialogModule,
    MatIconModule,
    MatTableModule,
    MatSidenavModule,
    MatButtonModule,
    MatTabsModule,
    MatTreeModule,
    HomePageModule
  ],
  exports: [
    UserAreaContainerComponent,
    UserIconComponent,
  ]
})
export class UserAreaModule { }
