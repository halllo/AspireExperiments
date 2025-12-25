import { ChangeDetectionStrategy, Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';

@Component({
  selector: 'app-header',
  standalone: true,
  template: `
    <nav class="navbar">
      <div class="navbar-left">
        <a class="navbar-brand" routerLink="/">Frontend</a>
        <div class="navbar-nav">
          @for (item of navigationItems; track $index) {
            <a
              [routerLink]="item.id"
              routerLinkActive="active-link"
              [routerLinkActiveOptions]="{ exact: item.id === '' }"
            >
              {{ item.label }}
            </a>
          }
        </div>
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

    .navbar-left {
      display: flex;
      align-items: center;
      gap: 2rem;
    }
    .navbar-brand {
      color: #fff;
      text-decoration: none;
      font-weight: bold;
      font-size: 1.2rem;
    }

    .navbar-nav {
      display: flex;
      align-items: center;
      gap: 1rem;
    }
    .navbar-nav a {
      color: #fff;
      text-decoration: none;
      padding: 0.5rem 1rem;
      border-radius: 4px;
      transition: background 0.2s, color 0.2s;
    }
    .navbar-nav a.active-link {
      background: rgba(255,255,255,0.10);
      color: #eee;
      font-weight: 500;
      box-shadow: none;
    }
    .navbar-nav a:hover {
      background: #eee;
      color: #222;
    }

    .navbar-menu {
      display: flex;
      align-items: center;
      gap: 1rem;
      margin-left: auto;
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
  imports: [CommonModule, RouterModule],
})
export class HeaderComponent {
  protected readonly navigationItems = [
    { id: '', label: 'Dashboard' },
    { id: 'settings', label: 'Settings' }
  ];
  isLoggedIn = signal(false);
  userProfile = signal<any>(null);

  constructor() {
    this.checkAuth();
  }

  private async checkAuth() {
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
