import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component } from '@angular/core';
import { AppConfigService } from '../shared/app-config.service';
import { FooterComponent } from './footer/footer.component';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss'],
  standalone: true,
  imports: [FooterComponent]
})
export class AppComponent {
  private readonly httpOptions = {
    headers: new HttpHeaders({
      Authorization: 'Bearer TopSecret'
    })
  };

  constructor(private readonly http: HttpClient, private readonly appConfig: AppConfigService) {}

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
