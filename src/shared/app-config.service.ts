import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

interface Config {
  build: string;
  commit: string;
  apiBaseUrl: string;
}

@Injectable()
export class AppConfigService {
  build = '';
  commit = '';
  apiBaseUrl = '';

  constructor(private readonly http: HttpClient) {}

  public load(): Promise<unknown> {
    return new Promise((resolve) => {
      this.http
        .get<Config>('./environment.json')
        .subscribe(config => {
          this.build = config.build;
          this.commit = config.commit;
          this.apiBaseUrl = config.apiBaseUrl;
          resolve(true);
        });
    });
  }
}
