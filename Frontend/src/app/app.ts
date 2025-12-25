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
      <router-outlet />
    </main>
  `,
  styles: `
    .main {
      margin: 1rem;
    }
  `
})
export class App {
  protected readonly title = signal('Frontend');
}
