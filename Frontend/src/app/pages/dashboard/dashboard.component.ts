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
      <p>
        @if (me.isLoading()) {
          Loading...
        } @else if (me.error()) {
          Error: {{ me.error() }}
        } @else {
          {{ me.value() | json }}
        }
      </p>
      <my-element></my-element>
    </section>
  `,
  styles: [``],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  changeDetection: ChangeDetectionStrategy.OnPush,
  imports: [JsonPipe],
})
export class DashboardComponent {
  private readonly http = inject(HttpClient);
  protected readonly me = httpResource(() => environment.apiPath + '/profile');
}
