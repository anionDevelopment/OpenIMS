import { ComponentFixture, TestBed } from '@angular/core/testing';

import { UserIconComponent } from './user-icon.component';
import { UserDataService } from '../../../services/user-data.service';
import { UserService } from '../../../generated/open-ims-backend';
import { of } from 'rxjs';
import { MatIconModule } from '@angular/material/icon';
import { MatMenuModule } from '@angular/material/menu';
import { MatTooltipModule } from '@angular/material/tooltip';

describe('UserIconComponent', () => {
  let component: UserIconComponent;
  let fixture: ComponentFixture<UserIconComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [
        MatIconModule,
        MatMenuModule,
        MatTooltipModule,
      ],
      declarations: [
        UserIconComponent,
      ],
      providers: [
        {
          provide: UserDataService,
          useValue: {
            getUserId: () => of("1"),
            getUserName: () => of("username"),
          }
        },
        {
          provide: UserService,
          useValue: {}
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(UserIconComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
