import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HintNotAuthenticatedComponent } from './hint-not-authenticated.component';
import { CommonModule } from '@angular/common';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { UserDataService } from '../../../services/user-data.service';

describe('HintNotAuthenticatedComponent', () => {
  let component: HintNotAuthenticatedComponent;
  let fixture: ComponentFixture<HintNotAuthenticatedComponent>;

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
        HintNotAuthenticatedComponent,
      ],
      providers: [
        {
          provide: UserDataService,
          useValue: {}
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(HintNotAuthenticatedComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
