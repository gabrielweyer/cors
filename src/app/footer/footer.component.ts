import { Component } from '@angular/core';
import { AppConfigService } from './../../shared/app-config.service';
import { SlicePipe } from '@angular/common';
import { LinkComponent } from '../link/link.component';

@Component({
  selector: 'app-footer',
  templateUrl: './footer.component.html',
  styleUrls: ['./footer.component.scss'],
  standalone: true,
  imports: [LinkComponent, SlicePipe]
})
export class FooterComponent {
  build: string;
  commit: string;

  constructor(appConfig: AppConfigService) {
    this.build = appConfig.build;
    this.commit = appConfig.commit;
  }
}
