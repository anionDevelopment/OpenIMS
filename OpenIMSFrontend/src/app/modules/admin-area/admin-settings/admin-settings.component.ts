import { Component, ChangeDetectionStrategy } from '@angular/core';
import { GeneralSettingsDTO, OpenIMSBackendService } from '../../../generated/open-ims-backend';
import { StorageService } from '../../../services/storage.service';

/**
 * Shows the settings which apply to the whole installation and which only an administrator may change.
 */
@Component({
  selector: 'app-admin-settings',
  standalone: false,
  templateUrl: './admin-settings.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './admin-settings.component.scss'
})
export class AdminSettingsComponent {
  registrationIsEnabled = false;

  constructor(private storageService: StorageService, private openIMSBackendService: OpenIMSBackendService) {
    this.openIMSBackendService.aPIV0OpenIMSBackendGetGeneralSettingsGet(this.storageService.getAccessToken()).subscribe(settings => {
      this.registrationIsEnabled = settings.registrationIsEnabled ?? false;
    });
  }

  onRegistrationIsEnabledChanged(enabled: boolean): void {
    this.registrationIsEnabled = enabled;
    const settings: GeneralSettingsDTO = { registrationIsEnabled: enabled };
    this.openIMSBackendService.aPIV0OpenIMSBackendSetGeneralSettingsPut(this.storageService.getAccessToken(), settings).subscribe();
  }
}
