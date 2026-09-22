
import { Component, ChangeDetectionStrategy } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterOutlet } from '@angular/router';
import { HomePageModule } from './modules/home-page/home-page.module';
import { UserAreaModule } from './modules/user-area/user-area.module';
import { AdminAreaModule } from './modules/admin-area/admin-area.module';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    RouterOutlet,
    HomePageModule,
    UserAreaModule,
    AdminAreaModule
],
  templateUrl: './app.component.html',
  changeDetection: ChangeDetectionStrategy.Eager,
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'OpenIMS';
}
