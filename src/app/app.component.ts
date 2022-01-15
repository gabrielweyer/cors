import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Component } from '@angular/core';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  private readonly httpOptions = {
    headers: new HttpHeaders({
      Authorization: 'Bearer TopSecret'
    })
  };

  constructor(private readonly http: HttpClient) {}

  onCacheCors(): void {
    this.invoke('https://ik8zfy1go7.execute-api.ap-southeast-2.amazonaws.com/prod/CorsCache');
  }

  onCacheCorsWithQueryString(): void {
    this.invoke('https://ik8zfy1go7.execute-api.ap-southeast-2.amazonaws.com/prod/CorsCache?hi=hello');
  }

  onCacheCorsWithDifferentPath(): void {
    this.invoke('https://ik8zfy1go7.execute-api.ap-southeast-2.amazonaws.com/prod/CorsCache/1234');
  }

  onCors(): void {
    this.invoke('https://ik8zfy1go7.execute-api.ap-southeast-2.amazonaws.com/prod/CorsNoCache');
  }

  onNoCors(): void {
    this.invoke('https://ik8zfy1go7.execute-api.ap-southeast-2.amazonaws.com/prod/NoCors');
  }

  private invoke(uri: string): void {
    this.http
      .get(uri, this.httpOptions)
      .subscribe((r: unknown) => console.log(r));
  }
}
