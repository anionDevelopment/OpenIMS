import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HomePageComponent } from './home-page.component';
import { CommonModule } from '@angular/common';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { UserService } from '../../../generated/open-ims-backend';
import { FrameWorkComponent } from '../frame-work/frame-work.component';
import { LoginFormComponent } from '../login-form/login-form.component';
import { FooterComponent } from '../footer/footer.component';
import { of } from 'rxjs';
import { UserDataService } from '../../../services/user-data.service';
import { StorageService } from '../../../services/storage.service';
import { ThemeService } from '../../../services/theme.service';
import { Router } from '@angular/router';

describe('HomePageComponent', () => {
  let component: HomePageComponent;
  let fixture: ComponentFixture<HomePageComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CommonModule,
        NoopAnimationsModule,
        ReactiveFormsModule,
        FormsModule,
        MatFormFieldModule,
        MatCardModule,
        MatInputModule,
        MatButtonModule,
        MatIconModule,
        MatSelectModule,
      ],
      declarations: [
        FrameWorkComponent,
        LoginFormComponent,
        FooterComponent,
        HomePageComponent,
      ],
      providers: [
        { provide: UserService, useValue: {} },
        { provide: StorageService, useValue: { hasAccessToken: () => false } },
        { provide: UserDataService, useValue: { userIsLoggedIn: () => of(false) } },
        { provide: ThemeService, useValue: { loadModeOfUser: vi.fn(), unloadModeOfUser: vi.fn() } },
        { provide: Router, useValue: { navigate: vi.fn() } },
      ]
    })
      .compileComponents();

    fixture = TestBed.createComponent(HomePageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
