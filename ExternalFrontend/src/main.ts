import { bootstrapApplication } from '@angular/platform-browser';
import { App } from './app';
import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';

const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners()
  ]
};

bootstrapApplication(App, appConfig)
  .catch((err) => console.error(err));
