import { Injectable } from '@angular/core';
import { StorageService } from './storage.service';
import { Observable, first, map, of, switchMap, tap } from 'rxjs';
import { UserInformationDTO, UserService } from '../generated/open-ims-backend';
import { ThemeService } from './theme.service';

@Injectable({
  providedIn: 'root'
})
export class UserDataService {
  private loaded = false;

  constructor(private userService: UserService, private storageService: StorageService, private themeService: ThemeService) {
  }

  loadUserData(): Observable<void> {
    return this.ensureLoaded();
  }

  unloadUserData() {
    this.storageService.setUserName(null);
    this.storageService.setUserId(null);
    this.storageService.setAccessToken(null);
    this.storageService.setUserIsAdmin(false);
    this.themeService.unloadModeOfUser();
    this.loaded = false;
  }

  getUserId(): Observable<string> {
    return this.ensureLoaded().pipe(map(() => {
      return this.storageService.getUserId();
    }));
  }

  getUserName(): Observable<string> {
    return this.ensureLoaded().pipe(map(() => {
      return this.storageService.getUserName();
    }));
  }

  userIsLoggedIn(): Observable<boolean> {
    if (this.storageService.hasAccessToken()) {
      return this.userService.aPIV1UserControllerTokenIsValidGet(this.storageService.getAccessToken());
    } else {
      return of(false);
    }
  }

  userIsAdmin(): Observable<boolean> {
    return this.ensureLoaded().pipe(map(() => {
      try {
        return this.storageService.getUserIsAdmin();
      } catch {
        return false;
      }
    }));
  }

  ensureLoaded(): Observable<void> {
    let pipe: Observable<void>;
    if (this.loaded) {
      pipe = of(void 0);//nothing to do
    } else {
      pipe = this.userService.aPIV1UserControllerGetUserInformationGet(this.storageService.getAccessToken()).pipe(
        tap((value: UserInformationDTO) => {
          this.storageService.setUserName(value.name);
          this.storageService.setUserId(value.id);
          this.storageService.setUserIsAdmin(value.isAdmin);
          this.loaded = true;
        }),
        switchMap(
          () => of(void 0)
        ));
    }
    return pipe.pipe(first());
  }
}
