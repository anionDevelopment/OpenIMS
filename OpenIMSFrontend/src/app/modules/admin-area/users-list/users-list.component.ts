import { Component, ChangeDetectionStrategy } from '@angular/core';
import { UserOverviewDTO, UserService } from '../../../generated/open-ims-backend';
import { StorageService } from '../../../services/storage.service';

@Component({
  selector: 'app-users-list',
  standalone: false,
  templateUrl: './users-list.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './users-list.component.scss'
})
export class UsersListComponent {
  users: UserOverviewDTO[] = [];
  allRoles: string[] = [];

  constructor(private storageService: StorageService, private userService: UserService) {
    this.reload();
  }

  private reload(): void {
    const accessToken = this.storageService.getAccessToken();
    this.userService.aPIV0UserControllerGetAllRolesGet(accessToken).subscribe(roles => this.allRoles = (roles ?? []).sort());
    this.userService.aPIV0UserControllerGetAllUsersGet(accessToken).subscribe(users => this.users = users ?? []);
  }

  userHasRole(user: UserOverviewDTO, role: string): boolean {
    return this.getRoleNamesOfUser(user).has(role);
  }

  /**
   * The generated client types the roles of a user as a Set, but a set is transported as a json-array, so what
   * really arrives at runtime is an array. Building a Set from it covers both and is therefore the only place
   * which has to know about that difference.
   */
  private getRoleNamesOfUser(user: UserOverviewDTO): Set<string> {
    return new Set<string>(user.roles ?? []);
  }

  onRoleToggled(user: UserOverviewDTO, role: string, checked: boolean): void {
    const roles = this.getRoleNamesOfUser(user);
    if (checked) {
      roles.add(role);
    } else {
      roles.delete(role);
    }
    user.roles = roles;
    const accessToken = this.storageService.getAccessToken();
    this.userService.aPIV0UserControllerSetRolesOfUserPut(accessToken, user.id ?? '', Array.from(roles)).subscribe();
  }
}
