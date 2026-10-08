import { Component, ChangeDetectionStrategy, EventEmitter, Input, Output } from '@angular/core';
import { FormControl } from '@angular/forms';
import { MatAutocompleteSelectedEvent } from '@angular/material/autocomplete';
import { Observable, debounceTime, distinctUntilChanged, filter, startWith, switchMap } from 'rxjs';
import { IdLabelDTO, OpenIMSBackendService } from '../../../generated/open-ims-backend';
import { StorageService } from '../../../services/storage.service';

/** The types of inventory-objects which can be searched. */
export type InventoryObjectType = 'Location' | 'Building' | 'Area' | 'Container' | 'Item';

/**
 * A dropdown which selects one inventory-object.
 *
 * The entries are not loaded completely in advance, because the inventory can be large: the entered text is sent
 * to the backend, which searches the names of the objects and returns only a limited amount of matching objects.
 * The search starts when the user stops typing for a moment, so that not every keystroke results in a request.
 *
 * Usage: <app-inventory-object-selector [types]="['Area', 'Container']" (objectSelected)="onSelected($event)">
 */
@Component({
  selector: 'app-inventory-object-selector',
  standalone: false,
  templateUrl: './inventory-object-selector.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './inventory-object-selector.component.scss'
})
export class InventoryObjectSelectorComponent {
  /** The time in milliseconds after the last keystroke after which the search is sent. */
  static readonly delayOfSearchInMilliseconds = 300;

  /** The types of the objects which can be selected. */
  @Input({ required: true }) types: InventoryObjectType[] = [];

  /** Emitted when the user selects an object. The input is cleared afterwards, so that the next object can be searched. */
  @Output() objectSelected = new EventEmitter<IdLabelDTO>();

  readonly searchInput = new FormControl<string | IdLabelDTO>('', { nonNullable: true });
  readonly searchResults$: Observable<IdLabelDTO[]>;

  constructor(private storageService: StorageService, private openIMSBackendService: OpenIMSBackendService) {
    this.searchResults$ = this.searchInput.valueChanges.pipe(
      startWith(''),
      /* After a selection the control contains the selected object and not a text, which must not start a search. */
      filter((value): value is string => typeof value === 'string'),
      debounceTime(InventoryObjectSelectorComponent.delayOfSearchInMilliseconds),
      distinctUntilChanged(),
      switchMap(searchText => this.search(searchText)),
    );
  }

  private search(searchText: string): Observable<IdLabelDTO[]> {
    return this.openIMSBackendService.aPIV0OpenIMSBackendSearchInventoryObjectsGet(this.storageService.getAccessToken(), searchText, this.types);
  }

  displayLabel(entry: IdLabelDTO | string | null): string {
    if (entry === null || typeof entry === 'string') {
      return entry ?? '';
    }
    return entry.label ?? '';
  }

  onOptionSelected(event: MatAutocompleteSelectedEvent): void {
    this.objectSelected.emit(event.option.value as IdLabelDTO);
    this.searchInput.setValue('');
  }
}
