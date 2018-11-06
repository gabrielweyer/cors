import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';

interface Config {
  build: string;
  commit: string;
}

@Injectable()
export class AppConfigService {
  build: string;
  commit: string;

  constructor(private readonly http: HttpClient) {}

  public load(): Promise<{}> {
    return new Promise((resolve) => {
      this.http
        .get<Config>('./environment.json')
        .subscribe(config => {
          this.build = config.build;
          this.commit = config.commit;
          resolve(true);
        });
    });
  }
}
