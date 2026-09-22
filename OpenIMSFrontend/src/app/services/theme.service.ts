import { effect, inject, Injectable } from '@angular/core';
import { NgxDarkmodeService, ThemeMode } from '@aniondev/ngx-darkmode-toggle-button';
import { StringValueDTO, UserService } from '../generated/open-ims-backend';
import { StorageService } from './storage.service';

/**
 * Keeps the color-scheme which the user chose in the backend.
 *
 * The component itself already stores the chosen mode in the localStorage of the browser, which is what makes it
 * work immediately and without a reload. Storing it additionally in the user-settings of the backend is what makes
 * the choice belong to the user instead of to one browser: it is applied again after a new login and on another
 * device. A user who never chose anything keeps the default "system", which follows the operating-system.
 */
@Injectable({ providedIn: 'root' })
export class ThemeService {

  private readonly darkmodeService = inject(NgxDarkmodeService);
  private readonly userService = inject(UserService);
  private readonly storageService = inject(StorageService);

  /** The mode which is currently stored in the backend. */
  private modeOfBackend: ThemeMode | null = null;

  /** Whether the mode of the user was loaded already. Before that nothing is sent to the backend. */
  private modeOfBackendWasLoaded = false;

  public constructor() {
    effect(() => {
      const mode: ThemeMode = this.darkmodeService.mode();
      // Nothing is sent before the stored mode was loaded, because the value of the component is only its
      // default until then and would overwrite the choice of the user in the backend. And a mode which was
      // just loaded is not sent back either.
      if (this.modeOfBackendWasLoaded && mode !== this.modeOfBackend) {
        this.storeMode(mode);
      }
    });
  }

  /** Loads the mode of the logged-in user and applies it. Does nothing if nobody is logged in. */
  public loadModeOfUser(): void {
    if (!this.storageService.hasAccessToken()) {
      return;
    }
    this.userService.aPIV0UserControllerGetThemeGet(this.storageService.getAccessToken()).subscribe((theme: StringValueDTO) => {
      this.modeOfBackend = theme.value as ThemeMode;
      this.modeOfBackendWasLoaded = true;
      this.darkmodeService.mode.set(this.modeOfBackend);
    });
  }

  /** Forgets what is known about the user. To be called when a user logs out. */
  public unloadModeOfUser(): void {
    this.modeOfBackend = null;
    this.modeOfBackendWasLoaded = false;
  }

  private storeMode(mode: ThemeMode): void {
    if (!this.storageService.hasAccessToken()) {
      return;
    }
    this.modeOfBackend = mode;
    this.userService.aPIV0UserControllerSetThemePut(this.storageService.getAccessToken(), { value: mode }).subscribe();
  }
}
