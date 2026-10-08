import { ComponentFixture, TestBed } from '@angular/core/testing';

import { FrameWorkComponent } from './frame-work.component';
import { CommonModule } from '@angular/common';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { FooterComponent } from '../footer/footer.component';
import { Router } from '@angular/router';
import { ThemeService } from '../../../services/theme.service';

describe('FrameWorkComponent', () => {
  let component: FrameWorkComponent;
  let fixture: ComponentFixture<FrameWorkComponent>;
  let themeServiceMock: { loadModeOfUser: ReturnType<typeof vi.fn>, unloadModeOfUser: ReturnType<typeof vi.fn> };
  let routerMock: { navigate: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    themeServiceMock = { loadModeOfUser: vi.fn(), unloadModeOfUser: vi.fn() };
    routerMock = { navigate: vi.fn() };

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
      ],
      declarations: [
        FooterComponent,
        FrameWorkComponent,
      ],
      providers: [
        { provide: Router, useValue: routerMock },
        { provide: ThemeService, useValue: themeServiceMock },
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(FrameWorkComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('applies the color-scheme of the user, because the frame is shown on every page', () => {
    expect(themeServiceMock.loadModeOfUser).toHaveBeenCalled();
  });

  it('navigates to the home-page when the header is clicked', () => {
    component.onHeaderClick();
    expect(routerMock.navigate).toHaveBeenCalledWith(['']);
  });
});
