import { ComponentFixture, TestBed } from '@angular/core/testing';
import { Component, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatDialog } from '@angular/material/dialog';
import { MatTreeModule } from '@angular/material/tree';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideHttpClient, withXhr } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { of } from 'rxjs';
import { vi } from 'vitest';
import { InventoryComponent } from './inventory.component';
import { InventoryTreeNodeDTO } from '../../../generated/open-ims-backend';
import { StorageService } from '../../../services/storage.service';

@Component({
  selector: 'app-frame-work',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  template: '<ng-content></ng-content>'
})
class MockFrameWorkComponent { }

@Component({
  selector: 'app-user-area-container',
  standalone: false,
  changeDetection: ChangeDetectionStrategy.Eager,
  template: '<ng-content></ng-content>'
})
class MockUserAreaContainerComponent { }

const inventoryOfTheSimulatedInstallation: InventoryTreeNodeDTO[] = [
  {
    id: 'location-1', type: 'Location', name: 'Headquarter', description: '', children: [
      {
        id: 'building-1', type: 'Building', name: 'Building A', description: '', children: [
          { id: 'area-1', type: 'Area', name: 'Room 101', description: '', children: [] },
        ]
      },
    ]
  },
];

describe('InventoryComponent', () => {
  let component: InventoryComponent;
  let fixture: ComponentFixture<InventoryComponent>;
  let httpTestingController: HttpTestingController;
  let dialogResult: unknown;

  beforeEach(async () => {
    new StorageService().setAccessToken('test-access-token');
    dialogResult = undefined;
    await TestBed.configureTestingModule({
      imports: [
        CommonModule,
        MatTreeModule,
        MatButtonModule,
        MatIconModule,
        NoopAnimationsModule,
      ],
      declarations: [
        MockFrameWorkComponent,
        MockUserAreaContainerComponent,
        InventoryComponent,
      ],
      providers: [
        { provide: MatDialog, useValue: { open: () => ({ afterClosed: () => of(dialogResult) }) } },
        provideHttpClient(withXhr()),
        provideHttpClientTesting(),
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(InventoryComponent);
    component = fixture.componentInstance;
    httpTestingController = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  afterEach(() => {
    vi.restoreAllMocks();
    new StorageService().removeAccessToken();
  });

  function answerTreeRequest(tree: InventoryTreeNodeDTO[]): void {
    httpTestingController.expectOne(request => request.url.endsWith('/GetInventoryTree')).flush(tree);
  }

  it('loads the inventory-tree', () => {
    answerTreeRequest(inventoryOfTheSimulatedInstallation);
    expect(component.locations).toEqual(inventoryOfTheSimulatedInstallation);
  });

  it('offers exactly the creations which the hierarchy allows', () => {
    expect(component.getCreatableChildTypes({ type: 'Location' })).toEqual(['Building', 'Area']);
    expect(component.getCreatableChildTypes({ type: 'Building' })).toEqual(['Area']);
    expect(component.getCreatableChildTypes({ type: 'Area' })).toEqual(['Container', 'Item']);
    expect(component.getCreatableChildTypes({ type: 'Container' })).toEqual(['Container', 'Item']);
    expect(component.getCreatableChildTypes({ type: 'Item' })).toEqual([]);
  });

  it('identifies the expansion of a node by the id of its object, so that it survives a reload', () => {
    expect(component.expansionKey({ id: 'building-1', name: 'Building A' })).toBe(component.expansionKey({ id: 'building-1', name: 'Renamed building' }));
  });

  it('creates an area in a building with the entered data and reloads the tree', () => {
    answerTreeRequest(inventoryOfTheSimulatedInstallation);
    dialogResult = { name: 'Room 102', description: 'Storage' };

    component.onCreateChild({ id: 'building-1', type: 'Building', name: 'Building A' }, 'Area');

    const request = httpTestingController.expectOne(request => request.url.endsWith('/CreateArea'));
    expect(request.request.params.get('parentId')).toBe('building-1');
    expect(request.request.body).toEqual({ name: 'Room 102', description: 'Storage' });
    request.flush({ value: 'area-2' });
    httpTestingController.expectOne(request => request.url.endsWith('/GetInventoryTree'));
  });

  it('creates an item with the endpoint for items', () => {
    answerTreeRequest(inventoryOfTheSimulatedInstallation);
    dialogResult = { name: 'Drill', description: '' };

    component.onCreateChild({ id: 'area-1', type: 'Area', name: 'Room 101' }, 'Item');

    expect(httpTestingController.expectOne(request => request.url.endsWith('/CreateItem')).request.params.get('parentId')).toBe('area-1');
  });

  it('does nothing when the dialog is cancelled', () => {
    answerTreeRequest(inventoryOfTheSimulatedInstallation);

    component.onCreateLocation();

    httpTestingController.expectNone(request => request.url.endsWith('/CreateLocation'));
  });

  it('changes name and description of an object', () => {
    answerTreeRequest(inventoryOfTheSimulatedInstallation);
    dialogResult = { name: 'Room 101a', description: 'Renovated' };

    component.onEdit({ id: 'area-1', type: 'Area', name: 'Room 101', description: '' });

    const request = httpTestingController.expectOne(request => request.url.endsWith('/UpdateInventoryObject'));
    expect(request.request.params.get('inventoryObjectId')).toBe('area-1');
    expect(request.request.body).toEqual({ name: 'Room 101a', description: 'Renovated' });
  });

  it('shows a rejected deletion and keeps the tree', () => {
    answerTreeRequest(inventoryOfTheSimulatedInstallation);
    vi.spyOn(window, 'confirm').mockReturnValue(true);

    component.onDelete({ id: 'building-1', type: 'Building', name: 'Building A' });

    httpTestingController.expectOne(request => request.url.endsWith('/DeleteInventoryObject')).flush('The building contains areas.', { status: 400, statusText: 'Bad Request' });
    expect(component.actionFailed).toBe(true);
    expect(component.locations).toEqual(inventoryOfTheSimulatedInstallation);
    httpTestingController.expectNone(request => request.url.endsWith('/GetInventoryTree'));
  });

  it('does not delete an object when the deletion is not confirmed', () => {
    answerTreeRequest(inventoryOfTheSimulatedInstallation);
    vi.spyOn(window, 'confirm').mockReturnValue(false);

    component.onDelete({ id: 'area-1', type: 'Area', name: 'Room 101' });

    httpTestingController.expectNone(request => request.url.endsWith('/DeleteInventoryObject'));
  });
});
