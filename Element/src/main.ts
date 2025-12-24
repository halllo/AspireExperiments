import { createApplication } from '@angular/platform-browser';
import { createCustomElement } from '@angular/elements';
import { MyComponent } from './my.component';

(async () => {

  const app = await createApplication({
    providers: [
    ],
  });

  const MyComponentElement = createCustomElement(MyComponent, {
    injector: app.injector
  });

  customElements.define('my-element', MyComponentElement);

})();