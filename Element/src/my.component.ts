import { JsonPipe } from '@angular/common';
import { httpResource } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, signal } from '@angular/core';
import { environment } from './environments/environment';

@Component({
  selector: 'my-component',
  template: `
    <p>{{ title() }}</p>
    @if (profile.isLoading()) {
      Loading...
    } @else if (profile.error()) {
      <pre class="scrollable-pre" style="color: red;">{{ profile.error() | json }}</pre>
    } @else {
      <pre class="scrollable-pre">{{ profile.value() | json }}</pre>
    }
  `,
  styles: [`
    p { color: blue; }
    
    .scrollable-pre {
      max-height: 300px;
      overflow: auto;
      background: #f8f8f8;
      border: 1px solid #ddd;
      padding: 1em;
      border-radius: 4px;
    }
  `],
  imports: [JsonPipe],
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class MyComponent {
  protected readonly title = signal('Hello from my component.');
  protected readonly profile = httpResource(() => ({
    url: environment.apiPath + '/profile',
    headers: {
      'X-CSRF': '1',
    },
    withCredentials: true,
  }));
}
