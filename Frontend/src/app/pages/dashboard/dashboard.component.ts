import { ChangeDetectionStrategy, Component, signal, inject, effect, computed, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { JsonPipe } from '@angular/common';
import { HttpClient, httpResource } from '@angular/common/http';
import { environment } from '../../../environments/environment.development';

@Component({
  selector: 'dashboard-root',
  template: `
    <section class="">
      <h1>Dashboard</h1>
      <p>Welcome to this experimental view.</p>
      @if (profile.isLoading()) {
        Loading...
      } @else if (profile.error()) {
        Error
      } @else {
        <pre class="scrollable-pre">{{ profile.value() | json }}</pre>
      }
      <my-element></my-element>
    </section>
  `,
  styles: [`
    .scrollable-pre {
      max-height: 300px;
      overflow: auto;
      background: #f8f8f8;
      border: 1px solid #ddd;
      padding: 1em;
      border-radius: 4px;
    }
  `],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [JsonPipe],
})
export class DashboardComponent {
  protected readonly profile = httpResource(() => environment.apiPath + '/profile');
}
