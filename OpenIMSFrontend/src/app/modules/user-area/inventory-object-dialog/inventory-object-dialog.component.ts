import { Component, ChangeDetectionStrategy, Inject } from '@angular/core';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { InventoryObjectDataDTO } from '../../../generated/open-ims-backend';

/** What the dialog displays when it is opened. */
export interface InventoryObjectDialogData {
  title: string;
  name: string;
  description: string;
}

/**
 * Asks for name and description of an inventory-object, when it is created or edited. It closes with the entered
 * data when the user confirms and without a result when the user cancels.
 */
@Component({
  selector: 'app-inventory-object-dialog',
  standalone: false,
  templateUrl: './inventory-object-dialog.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './inventory-object-dialog.component.scss'
})
export class InventoryObjectDialogComponent {
  readonly title: string;
  name: string;
  description: string;

  constructor(private dialogRef: MatDialogRef<InventoryObjectDialogComponent, InventoryObjectDataDTO>, @Inject(MAT_DIALOG_DATA) data: InventoryObjectDialogData) {
    this.title = data.title;
    this.name = data.name;
    this.description = data.description;
  }

  /** The backend rejects a name which is empty or consists of whitespace only, so it can not be confirmed here either. */
  nameIsValid(): boolean {
    return this.name.trim() !== '';
  }

  onConfirm(): void {
    this.dialogRef.close({ name: this.name.trim(), description: this.description });
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
