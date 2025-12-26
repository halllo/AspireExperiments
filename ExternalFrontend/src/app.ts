import { Component, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { environment } from './environments/environment';

@Component({
  selector: 'app-root',
  template: `
  <main class="main">
    <div class="content">
      <h1>External Frontend</h1>
      <my-element></my-element>
      <button (click)="csrfPost()">csrf post</button>
      <button (click)="csrfGet()">csrf get</button>
    </div>
  </main>`,
  styles: [``],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
})
export class App {

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
