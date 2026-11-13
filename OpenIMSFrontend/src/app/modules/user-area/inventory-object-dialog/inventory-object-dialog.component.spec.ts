import { ComponentFixture, TestBed } from '@angular/core/testing';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { FormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { vi } from 'vitest';
import { InventoryObjectDialogComponent, InventoryObjectDialogData } from './inventory-object-dialog.component';

describe('InventoryObjectDialogComponent', () => {
  let component: InventoryObjectDialogComponent;
  let fixture: ComponentFixture<InventoryObjectDialogComponent>;
  let close: ReturnType<typeof vi.fn>;

  beforeEach(async () => {
    close = vi.fn();
    const data: InventoryObjectDialogData = { title: 'Edit "Cupboard"', name: 'Cupboard', description: 'Grey' };
    await TestBed.configureTestingModule({
      imports: [
        MatDialogModule,
        FormsModule,
        MatFormFieldModule,
        MatInputModule,
        MatButtonModule,
        NoopAnimationsModule,
      ],
      declarations: [
        InventoryObjectDialogComponent,
      ],
      providers: [
        { provide: MatDialogRef, useValue: { close: close } },
        { provide: MAT_DIALOG_DATA, useValue: data },
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(InventoryObjectDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('displays the given values', () => {
    expect(component.title).toBe('Edit "Cupboard"');
    expect(component.name).toBe('Cupboard');
    expect(component.description).toBe('Grey');
  });

  it('does not accept a name which consists of whitespace only', () => {
    component.name = '   ';
    expect(component.nameIsValid()).toBe(false);
  });

  it('closes with the entered data and a trimmed name when confirmed', () => {
    component.name = '  Shelf  ';
    component.description = 'Next to the door';

    component.onConfirm();

    expect(close).toHaveBeenCalledWith({ name: 'Shelf', description: 'Next to the door' });
  });

  it('closes without a result when cancelled', () => {
    component.onCancel();

    expect(close).toHaveBeenCalledWith();
  });
});
