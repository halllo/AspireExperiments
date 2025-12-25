import { httpResource } from '@angular/common/http';
import { Component, signal } from '@angular/core';
import { environment } from './environments/environment';
import { JsonPipe } from '@angular/common';

@Component({
  selector: 'app-root',
  template: `
  <main class="main">
    <div class="content">
      <h1>External Frontend</h1>
      @if (profile.isLoading()) {
        Loading...
      } @else if (profile.error()) {
        <pre class="scrollable-pre" style="color: red;">{{ profile.error() | json }}</pre>
      } @else {
        <pre class="scrollable-pre">{{ profile.value() | json }}</pre>
      }
      <button (click)="csrfPost()">csrf post</button>
      <button (click)="csrfGet()">csrf get</button>
    </div>
  </main>`,
  styles: `
    .scrollable-pre {
      max-height: 300px;
      overflow: auto;
      background: #f8f8f8;
      border: 1px solid #ddd;
      padding: 1em;
      border-radius: 4px;
    }
  `,
  imports: [JsonPipe],
})
export class App {
  protected readonly profile = httpResource(() => environment.apiPath + '/profile');

  csrfPost() {
    const form = document.createElement('form');
    form.method = 'POST';
    form.action = environment.apiPath + '/echo';
    form.style.display = 'none';
    const input = document.createElement('input');
    input.type = 'hidden';
    input.name = 'malicious';
    input.value = 'true';
    form.appendChild(input);
    document.body.appendChild(form);
    form.submit();
  }

  csrfGet() {
    const img = document.createElement('img');
    img.src = environment.apiPath + '/profile';
    img.style.display = 'none';
    document.body.appendChild(img);
  }
}
