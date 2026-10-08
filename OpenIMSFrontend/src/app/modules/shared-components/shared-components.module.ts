import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { MatAutocompleteModule } from '@angular/material/autocomplete';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { InventoryObjectSelectorComponent } from './inventory-object-selector/inventory-object-selector.component';

/**
 * Components which are not part of a certain area of the application but are used by several pages, for example
 * the selection of an inventory-object.
 */
@NgModule({
  declarations: [
    InventoryObjectSelectorComponent,
  ],
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatAutocompleteModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  exports: [
    InventoryObjectSelectorComponent,
  ]
})
export class SharedComponentsModule { }
