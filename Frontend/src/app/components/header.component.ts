import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-header',
  standalone: true,
  template: `
    <nav class="navbar">
      <div class="navbar-brand">
        <a routerLink="/">Frontend</a>
      </div>
      <div class="navbar-menu">
        @if (isLoggedIn()) {
          <span>Welcome, {{ userProfile()?.name || 'User' }}</span>
          <button (click)="logout()">Logout</button>
        } @else {
          <button (click)="login()">Login</button>
        }
      </div>
    </nav>
  `,
  styles: [`
    .navbar {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1rem;
      background: #222;
      color: #fff;
    }

    .navbar-brand a {
      color: #fff;
      text-decoration: none;
      font-weight: bold;
      font-size: 1.2rem;
    }

    .navbar-menu {
      display: flex;
      align-items: center;
      gap: 1rem;
    }

    button {
      background: #fff;
      color: #222;
      border: none;
      padding: 0.5rem 1rem;
      border-radius: 4px;
      cursor: pointer;
    }
    button:hover {
      background: #eee;
    }
  `],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class HeaderComponent {
  private readonly router = inject(Router);
  isLoggedIn = signal(false);
  userProfile = signal<any>(null);

  constructor() {
    this.checkAuth();
  }

  async checkAuth() {
    try {
      const res = await fetch(environment.apiPath + '/profile', { credentials: 'include' });
      if (res.ok) {
        this.isLoggedIn.set(true);
        this.userProfile.set(await res.json());
      } else if (res.status !== 200) {
        this.isLoggedIn.set(false);
      }
    } catch {
      this.isLoggedIn.set(false);
    }
  }

  login() {
    document.location.href = environment.apiPath + '/login';
  }

  logout() {
    this.isLoggedIn.set(false);
    document.location.href = environment.apiPath + '/logout';
  }
}
