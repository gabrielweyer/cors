import { inject, provideAppInitializer, provideZonelessChangeDetection } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { AppConfigService } from './shared/app-config.service';

bootstrapApplication(AppComponent, {
  providers: [
    provideZonelessChangeDetection(),
    provideHttpClient(),
    provideAppInitializer(() => inject(AppConfigService).load())
  ]
})
  .catch(err => console.error(err));
