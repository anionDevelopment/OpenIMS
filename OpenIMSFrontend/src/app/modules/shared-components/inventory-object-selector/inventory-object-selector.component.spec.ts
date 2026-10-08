import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { provideHttpClient, withXhr } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { vi } from 'vitest';
import { InventoryObjectSelectorComponent } from './inventory-object-selector.component';
import { SharedComponentsModule } from '../shared-components.module';
import { StorageService } from '../../../services/storage.service';
import { IdLabelDTO } from '../../../generated/open-ims-backend';

describe('InventoryObjectSelectorComponent', () => {
  let component: InventoryObjectSelectorComponent;
  let fixture: ComponentFixture<InventoryObjectSelectorComponent>;
  let httpTestingController: HttpTestingController;

  beforeEach(async () => {
    vi.useFakeTimers();
    new StorageService().setAccessToken('test-access-token');
    await TestBed.configureTestingModule({
      imports: [
        SharedComponentsModule,
        NoopAnimationsModule,
      ],
      providers: [
        provideHttpClient(withXhr()),
        provideHttpClientTesting(),
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(InventoryObjectSelectorComponent);
    component = fixture.componentInstance;
    component.types = ['Area', 'Container'];
    httpTestingController = TestBed.inject(HttpTestingController);
    fixture.detectChanges();
  });

  afterEach(() => {
    vi.useRealTimers();
    new StorageService().removeAccessToken();
  });

  function expectSearchRequest(searchText: string) {
    return httpTestingController.expectOne(request => request.url.endsWith('/SearchInventoryObjects') && request.params.get('searchText') === searchText);
  }

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('searches the entered text with the configured types after the user stopped typing', () => {
    component.searchInput.setValue('box');
    vi.advanceTimersByTime(InventoryObjectSelectorComponent.delayOfSearchInMilliseconds);

    const request = expectSearchRequest('box');
    expect(request.request.params.getAll('types')).toEqual(['Area', 'Container']);
    request.flush([{ id: 'container-1', label: 'Big box' }]);
  });

  it('does not search while the user is still typing', () => {
    component.searchInput.setValue('b');
    vi.advanceTimersByTime(InventoryObjectSelectorComponent.delayOfSearchInMilliseconds / 2);
    component.searchInput.setValue('bo');
    vi.advanceTimersByTime(InventoryObjectSelectorComponent.delayOfSearchInMilliseconds);

    expectSearchRequest('bo');
    httpTestingController.expectNone(request => request.params.get('searchText') === 'b');
  });

  it('emits the selected object and clears the input', () => {
    const selected: IdLabelDTO[] = [];
    component.objectSelected.subscribe(entry => selected.push(entry));
    const entry: IdLabelDTO = { id: 'area-1', label: 'Store room' };

    component.onOptionSelected({ option: { value: entry } } as MatAutocompleteSelectedEvent);

    expect(selected).toEqual([entry]);
    expect(component.searchInput.value).toBe('');
  });

  it('displays the label of an object', () => {
    expect(component.displayLabel({ id: 'area-1', label: 'Store room' })).toBe('Store room');
    expect(component.displayLabel('typed text')).toBe('typed text');
    expect(component.displayLabel(null)).toBe('');
  });
});
