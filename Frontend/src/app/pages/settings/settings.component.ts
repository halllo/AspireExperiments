import { ChangeDetectionStrategy, Component, signal } from '@angular/core';

@Component({
  selector: 'settings-component',
  template: `
    <section class="">
      <h1>Settings</h1>
      <p>Adjust your preferences here.</p>
    </section>
  `,
  styles: ``,
})
export class SettingsComponent {
  // Add settings logic here using signals if needed
}
