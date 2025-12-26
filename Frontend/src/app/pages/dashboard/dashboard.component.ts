import { ChangeDetectionStrategy, Component, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';

@Component({
  selector: 'dashboard-root',
  template: `
    <section class="">
      <h1>Dashboard</h1>
      <p>Welcome to this experimental view.</p>
      <my-element></my-element>
    </section>
  `,
  styles: [``],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class DashboardComponent {
  
}
