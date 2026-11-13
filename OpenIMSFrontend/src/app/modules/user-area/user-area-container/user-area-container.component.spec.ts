import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserAreaContainerComponent } from './user-area-container.component';
import { UserDataService } from '../../../services/user-data.service';
import { Router } from '@angular/router';
import { of } from 'rxjs';
import { Component, ChangeDetectionStrategy } from '@angular/core';
import { MatDividerModule } from '@angular/material/divider';
import { UtilitiesService } from '../../../services/utilities.service';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatSelectModule } from '@angular/material/select';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatInputModule } from '@angular/material/input';
import { MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatSidenavModule } from '@angular/material/sidenav';
import { MatButtonModule } from '@angular/material/button';
import { MatTabsModule } from '@angular/material/tabs';
import { HomePageModule } from '../../home-page/home-page.module';

@Component({
  selector: 'app-user-icon',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  template: '<span>app-user-icon-container-mock</span>'
})
class MockUserIconComponent { }

describe('UserAreaContainerComponent', () => {
  let component: UserAreaContainerComponent;
  let fixture: ComponentFixture<UserAreaContainerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CommonModule,
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
        HomePageModule
      ],
      declarations: [
        MockUserIconComponent,
        UserAreaContainerComponent,
      ],
      providers: [
        {
          provide: UserDataService,
          useValue: {
            userIsAdmin: () => of(true),
          }
        },
        {
          provide: Router,
          useValue: {
            url: "user/settings",
            parseUrl: (url: string) => new Router().parseUrl(url),
          },
        },
        {
          provide: UtilitiesService,
          useValue: new UtilitiesService()
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(UserAreaContainerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('names the inventory-page', () => {
    expect(component.getSiteTitle('inventory')).toBe('Inventory');
  });

  it('rejects an unknown url-segment instead of showing an empty title', () => {
    expect(() => component.getSiteTitle('this-segment-does-not-exist')).toThrow();
  });
});
