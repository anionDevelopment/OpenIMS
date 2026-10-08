import { Component, ChangeDetectionStrategy } from '@angular/core';
import { Settings } from '../../../static/Settings';

@Component({
  selector: 'app-footer',
  standalone: false,
  templateUrl: './footer.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './footer.component.scss'
})
export class FooterComponent {
  public footer_text: string;
  constructor() {
    this.footer_text = `${Settings.getAppName()} v${Settings.getAppVersion()}`
  }
}
