import { Component, inject } from '@angular/core';
import { AppConfigService } from './../../shared/app-config.service';
import { SlicePipe } from '@angular/common';
import { LinkComponent } from '../link/link.component';

@Component({
  selector: 'app-footer',
  templateUrl: './footer.component.html',
  styleUrl: './footer.component.scss',
  imports: [LinkComponent, SlicePipe]
})
export class FooterComponent {
  build: string;
  commit: string;

  constructor() {
    const appConfig = inject(AppConfigService);

    this.build = appConfig.build;
    this.commit = appConfig.commit;
  }
}
