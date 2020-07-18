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
      Authorization: 'Bearer sob'
    })
  };

  constructor(private readonly http: HttpClient) {}

  onCacheCors(): void {
    this.invoke('https://ik8zfy1go7.execute-api.ap-southeast-2.amazonaws.com/sob/CorsCache');
  }

  onCacheCorsWithQueryString(): void {
    this.invoke('https://ik8zfy1go7.execute-api.ap-southeast-2.amazonaws.com/sob/CorsCache?hi=hello');
  }

  onCacheCorsWithDifferentPath(): void {
    this.invoke('https://ik8zfy1go7.execute-api.ap-southeast-2.amazonaws.com/sob/CorsCache/1234');
  }

  onCors(): void {
    this.invoke('https://ik8zfy1go7.execute-api.ap-southeast-2.amazonaws.com/sob/CorsNoCache');
  }

  onNoCors(): void {
    this.invoke('https://ik8zfy1go7.execute-api.ap-southeast-2.amazonaws.com/prod/NoCors');
  }

  private invoke(uri: string): void {
    this.http
      .get(uri, this.httpOptions)
      .subscribe((r: string) => console.log(r));
  }
}
