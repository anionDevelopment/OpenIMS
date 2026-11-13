import { Component, ChangeDetectionStrategy } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { Observable } from 'rxjs';
import { InventoryObjectDataDTO, InventoryTreeNodeDTO, OpenIMSBackendService } from '../../../generated/open-ims-backend';
import { StorageService } from '../../../services/storage.service';
import { InventoryObjectType } from '../../shared-components/inventory-object-selector/inventory-object-selector.component';
import { InventoryObjectDialogComponent, InventoryObjectDialogData } from '../inventory-object-dialog/inventory-object-dialog.component';

/**
 * The types which can be created directly in an object of a type. These are the rules of the inventory-hierarchy;
 * the backend enforces them anyway, here they only decide which actions are offered.
 */
const creatableChildTypes: Record<InventoryObjectType, InventoryObjectType[]> = {
  Location: ['Building', 'Area'],
  Building: ['Area'],
  Area: ['Container', 'Item'],
  Container: ['Container', 'Item'],
  Item: [],
};

/**
 * Displays the complete inventory as a tree and lets the user model it: create locations and, inside an object,
 * the objects which the hierarchy allows there, edit name and description of an object and delete an object.
 */
@Component({
  selector: 'app-inventory',
  standalone: false,
  templateUrl: './inventory.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './inventory.component.scss'
})
export class InventoryComponent {
  locations: InventoryTreeNodeDTO[] = [];
  actionFailed = false;

  readonly childrenAccessor = (node: InventoryTreeNodeDTO): InventoryTreeNodeDTO[] => node.children ?? [];

  /** The expansion is identified by the id, so that an expanded object stays expanded when the tree is reloaded with new node-objects. */
  readonly expansionKey = (node: InventoryTreeNodeDTO): string => node.id!;

  readonly hasChildren = (_: number, node: InventoryTreeNodeDTO): boolean => (node.children ?? []).length > 0;

  constructor(private storageService: StorageService, private openIMSBackendService: OpenIMSBackendService, private dialog: MatDialog) {
    this.reloadTree();
  }

  private reloadTree(): void {
    this.openIMSBackendService.aPIV1OpenIMSBackendGetInventoryTreeGet(this.storageService.getAccessToken()).subscribe(locations => this.locations = locations ?? []);
  }

  /** Every action reports a failure the same way, so that a rejected action (for example deleting a building which still has rooms) is visible. */
  private observer(onSuccess: () => void) {
    this.actionFailed = false;
    return {
      next: () => onSuccess(),
      error: () => this.actionFailed = true,
    };
  }

  getCreatableChildTypes(node: InventoryTreeNodeDTO): InventoryObjectType[] {
    return creatableChildTypes[node.type as InventoryObjectType];
  }

  getLabelOfType(type: InventoryObjectType): string {
    switch (type) {
      case 'Location': return $localize`:@@inventory_type_location:Location`;
      case 'Building': return $localize`:@@inventory_type_building:Building`;
      case 'Area': return $localize`:@@inventory_type_area:Area`;
      case 'Container': return $localize`:@@inventory_type_container:Container`;
      case 'Item': return $localize`:@@inventory_type_item:Item`;
    }
  }

  getIconOfType(type: InventoryObjectType): string {
    switch (type) {
      case 'Location': return 'place';
      case 'Building': return 'business';
      case 'Area': return 'meeting_room';
      case 'Container': return 'inventory_2';
      case 'Item': return 'category';
    }
  }

  onCreateLocation(): void {
    this.askForData($localize`:@@inventory_create_location_title:Create location`, '', '', data => this.openIMSBackendService.aPIV1OpenIMSBackendCreateLocationPost(this.storageService.getAccessToken(), data));
  }

  onCreateChild(parent: InventoryTreeNodeDTO, type: InventoryObjectType): void {
    const title = $localize`:@@inventory_create_child_title:Create ${this.getLabelOfType(type)}:type: in "${parent.name}:parentName:"`;
    this.askForData(title, '', '', data => this.createChild(parent.id!, type, data));
  }

  onEdit(node: InventoryTreeNodeDTO): void {
    const title = $localize`:@@inventory_edit_title:Edit "${node.name}:name:"`;
    this.askForData(title, node.name ?? '', node.description ?? '', data => this.openIMSBackendService.aPIV1OpenIMSBackendUpdateInventoryObjectPut(this.storageService.getAccessToken(), node.id!, data));
  }

  onDelete(node: InventoryTreeNodeDTO): void {
    if (!confirm($localize`:@@inventory_delete_confirmation:Delete "${node.name}:name:"? Only an object which contains nothing can be deleted.`)) {
      return;
    }
    this.openIMSBackendService.aPIV1OpenIMSBackendDeleteInventoryObjectDelete(this.storageService.getAccessToken(), node.id!).subscribe(this.observer(() => this.reloadTree()));
  }

  private createChild(parentId: string, type: InventoryObjectType, data: InventoryObjectDataDTO): Observable<unknown> {
    const accessToken = this.storageService.getAccessToken();
    switch (type) {
      case 'Building': return this.openIMSBackendService.aPIV1OpenIMSBackendCreateBuildingPost(accessToken, parentId, data);
      case 'Area': return this.openIMSBackendService.aPIV1OpenIMSBackendCreateAreaPost(accessToken, parentId, data);
      case 'Container': return this.openIMSBackendService.aPIV1OpenIMSBackendCreateContainerPost(accessToken, parentId, data);
      case 'Item': return this.openIMSBackendService.aPIV1OpenIMSBackendCreateItemPost(accessToken, parentId, data);
      case 'Location': throw new Error('A location can not be created inside another object.');
    }
  }

  /** Opens the dialog and, when the user confirms it, executes the action with the entered data and reloads the tree. */
  private askForData(title: string, name: string, description: string, action: (data: InventoryObjectDataDTO) => Observable<unknown>): void {
    const dialogData: InventoryObjectDialogData = { title: title, name: name, description: description };
    this.dialog.open<InventoryObjectDialogComponent, InventoryObjectDialogData, InventoryObjectDataDTO>(InventoryObjectDialogComponent, { data: dialogData }).afterClosed().subscribe(data => {
      if (data !== undefined) {
        action(data).subscribe(this.observer(() => this.reloadTree()));
      }
    });
  }
}
