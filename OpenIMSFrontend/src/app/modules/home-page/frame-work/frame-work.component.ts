import { Component, OnInit, inject, ChangeDetectionStrategy } from '@angular/core';
import { Settings } from '../../../static/Settings';
import { Router } from '@angular/router';
import { ThemeService } from '../../../services/theme.service';

@Component({
  selector: 'app-frame-work',
  standalone: false,
  templateUrl: './frame-work.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './frame-work.component.scss'
})
export class FrameWorkComponent implements OnInit {
  title: string;
  private readonly themeService = inject(ThemeService);
  constructor(private router: Router) {
    this.title = Settings.getAppName();
  }
  ngOnInit(): void {
    // The frame is shown on every page, so this is the place where the color-scheme of the user is applied
    // regardless of which page the user enters the application with.
    this.themeService.loadModeOfUser();
  }
  onHeaderClick() {
    this.router.navigate(['']);
  }

}
