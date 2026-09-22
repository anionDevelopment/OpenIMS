import { ComponentFixture, TestBed } from '@angular/core/testing';
import { AdminAreaContainerComponent } from './admin-area-container.component';
import { CommonModule } from '@angular/common';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';
import { MatDividerModule } from '@angular/material/divider';

describe('AdminAreaContainerComponent', () => {
  let component: AdminAreaContainerComponent;
  let fixture: ComponentFixture<AdminAreaContainerComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        CommonModule,
        NoopAnimationsModule,
        ReactiveFormsModule,
        FormsModule,
        MatFormFieldModule,
        MatDividerModule,
        MatCardModule,
        MatInputModule,
        MatButtonModule,
        MatIconModule,
      ],
      declarations: [
        AdminAreaContainerComponent,
      ],
      providers: [
        {
          provide: Router,
          useValue: {
            url: "admin/dashboard",
          },
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AdminAreaContainerComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
