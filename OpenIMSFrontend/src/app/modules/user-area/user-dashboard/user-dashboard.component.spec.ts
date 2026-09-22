import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserDashboardComponent } from './user-dashboard.component';
import { UserDataService } from '../../../services/user-data.service';
import { of } from 'rxjs';
import { HomePageModule } from '../../home-page/home-page.module';
import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ThemeService } from '../../../services/theme.service';

@Component({
  selector: 'app-user-area-container',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  template: '<span>app-user-area-container-mock</span>'
})
class MockUserAreaContainerComponent { }

describe('UserDashboardComponent', () => {
  let component: UserDashboardComponent;
  let fixture: ComponentFixture<UserDashboardComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        HomePageModule,
      ],
      declarations: [
        MockUserAreaContainerComponent,
        UserDashboardComponent,
      ],
      providers: [
        {
          provide: UserDataService,
          useValue: {
            getUserName: () => of("username"),
          }
        },
        {
          // the page is rendered inside the frame, which applies the color-scheme of the user; that is not
          // what this testcase is about, so the service is replaced instead of a backend being simulated.
          provide: ThemeService,
          useValue: { loadModeOfUser: vi.fn(), unloadModeOfUser: vi.fn() }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(UserDashboardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
