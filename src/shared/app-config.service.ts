import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';

interface Config {
  build: string;
  commit: string;
  apiBaseUrl: string;
}

@Injectable()
export class AppConfigService {
  private readonly http = inject(HttpClient);

  build = '';
  commit = '';
  apiBaseUrl = '';

  constructor() {}

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
