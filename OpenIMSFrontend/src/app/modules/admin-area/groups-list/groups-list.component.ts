import { Component, ChangeDetectionStrategy } from '@angular/core';
import { IdLabelDTO, OpenIMSBackendService, UserService } from '../../../generated/open-ims-backend';
import { StorageService } from '../../../services/storage.service';
import { InventoryObjectType } from '../../shared-components/inventory-object-selector/inventory-object-selector.component';

/**
 * Manages the groups: an administrator creates and deletes groups and decides which inventory-objects the members
 * of a group are authorized to view. Which users are members of which group is decided on the users-page.
 */
@Component({
  selector: 'app-groups-list',
  standalone: false,
  templateUrl: './groups-list.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './groups-list.component.scss'
})
export class GroupsListComponent {
  /** Every inventory-object can be assigned to a group. */
  readonly assignableTypes: InventoryObjectType[] = ['Location', 'Building', 'Area', 'Container', 'Item'];

  groups: string[] = [];
  selectedGroup: string | null = null;
  itemsOfSelectedGroup: IdLabelDTO[] = [];
  nameOfNewGroup = '';
  actionFailed = false;

  constructor(private storageService: StorageService, private userService: UserService, private openIMSBackendService: OpenIMSBackendService) {
    this.reloadGroups();
  }

  private reloadGroups(): void {
    this.userService.aPIV1UserControllerGetAllRolesGet(this.storageService.getAccessToken()).subscribe(groups => this.groups = (groups ?? []).sort());
  }

  private reloadItemsOfSelectedGroup(): void {
    if (this.selectedGroup === null) {
      this.itemsOfSelectedGroup = [];
      return;
    }
    this.openIMSBackendService.aPIV1OpenIMSBackendGetAuthorizableItemsOfGroupGet(this.storageService.getAccessToken(), this.selectedGroup).subscribe(items => this.itemsOfSelectedGroup = items ?? []);
  }

  /** Every action reports a failure the same way, so that a rejected action (for example deleting a group which OpenIMS requires) is visible. */
  private observer(onSuccess: () => void) {
    this.actionFailed = false;
    return {
      next: () => onSuccess(),
      error: () => this.actionFailed = true,
    };
  }

  onGroupSelected(group: string): void {
    this.selectedGroup = group;
    this.reloadItemsOfSelectedGroup();
  }

  onCreateGroup(): void {
    const groupName = this.nameOfNewGroup.trim();
    this.userService.aPIV1UserControllerCreateGroupPost(this.storageService.getAccessToken(), { value: groupName }).subscribe(this.observer(() => {
      this.nameOfNewGroup = '';
      this.reloadGroups();
    }));
  }

  onDeleteGroup(group: string): void {
    if (!confirm($localize`:@@groups_delete_confirmation:Delete the group "${group}:groupName:"? Its members lose the access which they have through this group.`)) {
      return;
    }
    this.userService.aPIV1UserControllerDeleteGroupDelete(this.storageService.getAccessToken(), group).subscribe(this.observer(() => {
      if (this.selectedGroup === group) {
        this.selectedGroup = null;
        this.reloadItemsOfSelectedGroup();
      }
      this.reloadGroups();
    }));
  }

  onItemAdded(item: IdLabelDTO): void {
    this.openIMSBackendService.aPIV1OpenIMSBackendAssignGroupToAuthorizableItemPut(this.storageService.getAccessToken(), this.selectedGroup!, item.id!).subscribe(this.observer(() => this.reloadItemsOfSelectedGroup()));
  }

  onItemRemoved(item: IdLabelDTO): void {
    this.openIMSBackendService.aPIV1OpenIMSBackendRemoveGroupFromAuthorizableItemPut(this.storageService.getAccessToken(), this.selectedGroup!, item.id!).subscribe(this.observer(() => this.reloadItemsOfSelectedGroup()));
  }
}
