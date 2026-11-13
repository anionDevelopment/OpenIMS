import { ComponentFixture, TestBed } from '@angular/core/testing';

import { LoginFormComponent } from './login-form.component';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatCardModule } from '@angular/material/card';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';
import { UserService } from '../../../generated/open-ims-backend';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { UserDataService } from '../../../services/user-data.service';
import { StorageService } from '../../../services/storage.service';

describe('LoginFormComponent', () => {
  let component: LoginFormComponent;
  let fixture: ComponentFixture<LoginFormComponent>;
  let userServiceMock: { aPIV1UserControllerLoginPut: ReturnType<typeof vi.fn> };
  let storageServiceMock: { setAccessToken: ReturnType<typeof vi.fn> };
  let routerMock: { navigate: ReturnType<typeof vi.fn> };

  beforeEach(async () => {
    userServiceMock = { aPIV1UserControllerLoginPut: vi.fn() };
    storageServiceMock = { setAccessToken: vi.fn() };
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
        MatSelectModule,
      ],
      declarations: [
        LoginFormComponent,
      ],
      providers: [
        { provide: UserService, useValue: userServiceMock },
        { provide: StorageService, useValue: storageServiceMock },
        { provide: UserDataService, useValue: { loadUserData: () => of(void 0) } },
        { provide: Router, useValue: routerMock },
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginFormComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('stores the access-token and navigates to the dashboard on a successful login', () => {
    // arrange
    userServiceMock.aPIV1UserControllerLoginPut.mockReturnValue(of({ value: 'the-access-token' }));
    component.form.setValue({ username: 'alice', password: 'pa55w0rd' });

    // act
    component.login();

    // assert
    expect(userServiceMock.aPIV1UserControllerLoginPut).toHaveBeenCalledWith('alice', 'pa55w0rd');
    expect(storageServiceMock.setAccessToken).toHaveBeenCalledWith('the-access-token');
    expect(routerMock.navigate).toHaveBeenCalledWith(['user', 'dashboard']);
    expect(component.loginFailed).toBe(false);
  });

  it('shows a hint and does not navigate when the credentials were rejected', () => {
    // arrange
    userServiceMock.aPIV1UserControllerLoginPut.mockReturnValue(throwError(() => ({ status: 401 })));
    component.form.setValue({ username: 'alice', password: 'wrong-password' });

    // act
    component.login();

    // assert
    expect(component.loginFailed).toBe(true);
    expect(routerMock.navigate).not.toHaveBeenCalled();
    // a rejected login must not leave a token behind which a following request would send.
    expect(storageServiceMock.setAccessToken).toHaveBeenCalledWith(null);
  });
});
