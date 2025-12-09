

import { Routes } from '@angular/router';
import { DashboardComponent } from './pages/dashboard/dashboard.component';
import { SettingsComponent } from './pages/settings/settings.component';

export const routes: Routes = [
    { path: 'settings', component: SettingsComponent, title: 'Settings' },
    {
        path: '',
        pathMatch: 'full',
        component: DashboardComponent,
    },
    {
        path: 'dashboard',
        component: DashboardComponent,
    },
];
