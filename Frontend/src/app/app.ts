import { Component, CUSTOM_ELEMENTS_SCHEMA, signal, inject } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { HeaderComponent } from './components/header.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, HeaderComponent],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  template: `
    <app-header />
    <main class="main">
      @for (item of navigationItems; track $index) {
        <button (click)="navigate(item.id)">{{ item.label }}</button>
      }
      <router-outlet />
    </main>
  `,
  styles: `
  `
})
export class App {
  private readonly router = inject(Router);
  protected readonly title = signal('Frontend');
  protected readonly navigationItems = [
    { id: '', label: 'Dashboard', icon: 'fa-regular fa-chart-line' },
    { id: 'settings', label: 'Settings', icon: 'fa-regular fa-gear' }
  ];

  navigate(target: string) {
    this.router.navigate([target]);
  }
}
