import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HintNotAuthorizedComponent } from './hint-not-authorized.component';
import { CommonModule } from '@angular/common';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';

describe('HintNotAuthorizedComponent', () => {
  let component: HintNotAuthorizedComponent;
  let fixture: ComponentFixture<HintNotAuthorizedComponent>;

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
        HintNotAuthorizedComponent,
      ],
      providers: [
      ]

    })
      .compileComponents();

    fixture = TestBed.createComponent(HintNotAuthorizedComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
