import { ComponentFixture, TestBed } from '@angular/core/testing';
import { GroupsListComponent } from './groups-list.component';
import { FrameWorkComponent } from '../../home-page/frame-work/frame-work.component';
import { AdminAreaContainerComponent } from '../admin-area-container/admin-area-container.component';
import { CommonModule } from '@angular/common';
import { HomePageModule } from '../../home-page/home-page.module';
import { SharedComponentsModule } from '../../shared-components/shared-components.module';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatDividerModule } from '@angular/material/divider';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { Router } from '@angular/router';
import { provideHttpClient, withXhr } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { vi } from 'vitest';
import { StorageService } from '../../../services/storage.service';

describe('GroupsListComponent', () => {
  let component: GroupsListComponent;
  let fixture: ComponentFixture<GroupsListComponent>;
  let httpTestingController: HttpTestingController;

  beforeEach(async () => {
    new StorageService().setAccessToken('test-access-token');
    await TestBed.configureTestingModule({
      imports: [
        CommonModule,
        HomePageModule,
        SharedComponentsModule,
        NoopAnimationsModule,
        FormsModule,
        MatFormFieldModule,
        MatDividerModule,
        MatInputModule,
        MatButtonModule,
        MatIconModule,
      ],
      declarations: [
        FrameWorkComponent,
        AdminAreaContainerComponent,
      ],
      providers: [
        {
          provide: Router,
          useValue: {
            url: "admin/groups",
          },
        },
        provideHttpClient(withXhr()),
        provideHttpClientTesting(),
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(GroupsListComponent);
    component = fixture.componentInstance;
    httpTestingController = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  afterEach(() => {
    vi.restoreAllMocks();
    new StorageService().removeAccessToken();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('lists the groups sorted by name', () => {
    httpTestingController.expectOne(request => request.url.endsWith('/GetAllRoles')).flush(['Workshop', 'Administrators', 'Users']);
    expect(component.groups).toEqual(['Administrators', 'Users', 'Workshop']);
  });

  it('loads the objects of the selected group', () => {
    component.onGroupSelected('Workshop');

    const request = httpTestingController.expectOne(request => request.url.endsWith('/GetAuthorizableItemsOfGroup'));
    expect(request.request.params.get('groupName')).toBe('Workshop');
    request.flush([{ id: 'area-1', label: 'Store room' }]);
    expect(component.itemsOfSelectedGroup).toEqual([{ id: 'area-1', label: 'Store room' }]);
  });

  it('assigns a selected object to the selected group and reloads its objects', () => {
    component.onGroupSelected('Workshop');
    httpTestingController.expectOne(request => request.url.endsWith('/GetAuthorizableItemsOfGroup')).flush([]);

    component.onItemAdded({ id: 'area-1', label: 'Store room' });

    const request = httpTestingController.expectOne(request => request.url.endsWith('/AssignGroupToAuthorizableItem'));
    expect(request.request.params.get('groupName')).toBe('Workshop');
    expect(request.request.params.get('authorizableItemId')).toBe('area-1');
    request.flush(null);
    httpTestingController.expectOne(request => request.url.endsWith('/GetAuthorizableItemsOfGroup'));
  });

  it('shows a failure of an action', () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true);

    component.onDeleteGroup('Users');

    httpTestingController.expectOne(request => request.url.endsWith('/DeleteGroup')).flush('The group is required.', { status: 400, statusText: 'Bad Request' });
    expect(component.actionFailed).toBe(true);
  });

  it('does not delete a group when the deletion is not confirmed', () => {
    vi.spyOn(window, 'confirm').mockReturnValue(false);

    component.onDeleteGroup('Workshop');

    httpTestingController.expectNone(request => request.url.endsWith('/DeleteGroup'));
  });
});
