import { ComponentFixture, TestBed } from '@angular/core/testing';
import { UserSettingsComponent } from './user-settings.component';
import { CommonModule } from '@angular/common';
import { MatDividerModule } from '@angular/material/divider';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { HomePageModule } from '../../home-page/home-page.module';
import { MatIconModule } from '@angular/material/icon';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';
import { UserService } from '../../../generated/open-ims-backend';
import { Router } from '@angular/router';
import { StorageService } from '../../../services/storage.service';
import { of } from 'rxjs';
import { UserIconComponent } from '../user-icon/user-icon.component';
import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NgxDarkmodeToggleButtonComponent } from '@aniondev/ngx-darkmode-toggle-button';
import { NgxCultureSelectorComponent } from '@aniondev/ngx-culture-selector';

@Component({
  selector: 'app-user-area-container',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  template: '<ng-content></ng-content>'
})
class MockUserAreaContainerComponent { }

describe('UserSettingsComponent', () => {
  let component: UserSettingsComponent;
  let fixture: ComponentFixture<UserSettingsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CommonModule,
        MatDividerModule,
        MatMenuModule,
        MatTooltipModule,
        HomePageModule,
        MatIconModule,
        MatSidenavModule,
        MatButtonModule,
        MatTabsModule,
        NgxDarkmodeToggleButtonComponent,
        NgxCultureSelectorComponent,
      ],
      declarations: [
        UserIconComponent,
        MockUserAreaContainerComponent,
        UserSettingsComponent,
      ],
      providers: [
        {
          provide: StorageService,
          useValue: {
            getAccessToken: () => "accesstoken1",
            hasAccessToken: () => false,
          }
        },
        {
          provide: UserService,
          useValue: {
            aPIV0UserControllerGetUserInformationGet: () => of({
              id: "id1",
              name: "admin",
              isAdmin: true,
            })
          }
        },
        {
          provide: Router,
          useValue: {
            url: 'user/settings'
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(UserSettingsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
