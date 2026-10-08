import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HintNotFoundComponent } from './hint-not-found.component';
import { CommonModule } from '@angular/common';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { FrameWorkComponent } from '../frame-work/frame-work.component';
import { FooterComponent } from '../footer/footer.component';
import { Router } from '@angular/router';
import { ThemeService } from '../../../services/theme.service';

describe('HintNotFoundComponent', () => {
  let component: HintNotFoundComponent;
  let fixture: ComponentFixture<HintNotFoundComponent>;

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
      ],
      declarations: [
        FrameWorkComponent,
        FooterComponent,
        HintNotFoundComponent,
      ],
      providers: [
        { provide: Router, useValue: { navigate: vi.fn() } },
        { provide: ThemeService, useValue: { loadModeOfUser: vi.fn(), unloadModeOfUser: vi.fn() } },
      ]

    })
      .compileComponents();

    fixture = TestBed.createComponent(HintNotFoundComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
