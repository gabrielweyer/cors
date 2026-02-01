import { enableProdMode, importProvidersFrom, inject, provideAppInitializer, provideZoneChangeDetection } from '@angular/core';
import { environment } from './environments/environment';
import { AppComponent } from './app/app.component';
import { withInterceptorsFromDi, provideHttpClient } from '@angular/common/http';
import { BrowserModule, bootstrapApplication } from '@angular/platform-browser';
import { AppConfigService } from './shared/app-config.service';

if (environment.production) {
  enableProdMode();
}

bootstrapApplication(AppComponent, {
  providers: [
    provideZoneChangeDetection(),importProvidersFrom(BrowserModule),
    AppConfigService,
    provideAppInitializer(() => {
      const config = inject(AppConfigService);
      return config.load();
    }),
    provideHttpClient(withInterceptorsFromDi())
  ]
})
  .catch(err => console.error(err));
