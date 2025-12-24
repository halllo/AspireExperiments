import { ChangeDetectionStrategy, Component, signal, inject, effect, computed, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'dashboard-root',
  template: `
    <section class="">
      <h1>Frontend</h1>
      <p>Welcome to this experimental view.</p>
      <my-element></my-element>
    </section>
  `,
  styles: [``],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class DashboardComponent {
  private readonly http = inject(HttpClient);
}
