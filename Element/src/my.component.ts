import { Component, signal } from '@angular/core';

@Component({
  selector: 'my-component',
  template: `<p>{{ title() }}</p>`,
  styles: [`p { color: blue; }`]
})
export class MyComponent {
  protected readonly title = signal('Hello from my component.');
}
