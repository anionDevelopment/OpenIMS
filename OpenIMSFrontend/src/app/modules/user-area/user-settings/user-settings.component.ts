import { Component, ChangeDetectionStrategy } from '@angular/core';
import { NgxCultureOption } from '@aniondev/ngx-culture-selector';

/**
 * Shows the settings which a user can change for themselves.
 */
@Component({
  selector: 'app-user-settings',
  standalone: false,
  templateUrl: './user-settings.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './user-settings.component.scss'
})
export class UserSettingsComponent {

  /**
   * The cultures which the culture-selector offers. This list is fixed on purpose and independent of the
   * translations which the application actually contains, because it currently only serves to try out the
   * culture-selector.
   */
  cultures: NgxCultureOption[] = [
    { culture: 'en', label: 'English' },
    { culture: 'de', label: 'German' },
    { culture: 'es', label: 'Spanish' },
  ];

  /** The culture which is currently chosen. The choice is not applied to the application yet. */
  selectedCulture = 'en';

  cultureSelected(culture: string): void {
    this.selectedCulture = culture;
  }
}
