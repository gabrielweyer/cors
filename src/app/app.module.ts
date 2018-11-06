import { HttpClientModule } from '@angular/common/http';
import { APP_INITIALIZER, NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { AppConfigService } from './../shared/app-config.service';
import { AppComponent } from './app.component';
import { FooterComponent } from './footer/footer.component';
import { LinkComponent } from './link/link.component';

@NgModule({
  declarations: [
    AppComponent,
    FooterComponent,
    LinkComponent
  ],
  imports: [
    BrowserModule,
    HttpClientModule
  ],
  providers: [
    AppConfigService,
    { provide: APP_INITIALIZER, useFactory: (config: AppConfigService) => () => config.load(), deps: [AppConfigService], multi: true }
  ],
  bootstrap: [
    AppComponent
  ]
})
export class AppModule { }
