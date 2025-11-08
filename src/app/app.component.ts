import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { AppConfigService } from '../shared/app-config.service';
import { FooterComponent } from './footer/footer.component';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss',
  imports: [FooterComponent]
})
export class AppComponent {
  private readonly http = inject(HttpClient);
  private readonly appConfig = inject(AppConfigService);

  private readonly httpOptions = {
    headers: new HttpHeaders({
      Authorization: 'Bearer TopSecret'
    })
  };

  onCacheCors(): void {
    this.invoke('/cors-cache');
  }

  onCacheCorsWithQueryString(): void {
    this.invoke('/cors-cache?hi=hello');
  }

  onCacheCorsWithDifferentPath(): void {
    this.invoke('/cors-cache/1234');
  }

  onCors(): void {
    this.invoke('/cors-no-cache');
  }

  onNoCors(): void {
    this.invoke('/no-cors');
  }

  private invoke(path: string): void {
    this.http
      .get(`${this.appConfig.apiBaseUrl}${path}`, this.httpOptions)
      .subscribe((r: unknown) => console.log(r));
  }
}
