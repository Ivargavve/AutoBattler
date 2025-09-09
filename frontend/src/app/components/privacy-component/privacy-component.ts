import { Component, OnInit, OnDestroy } from '@angular/core';
import { TitleService } from '../../services/title.service';

@Component({
  selector: 'app-privacy',
  templateUrl: './privacy-component.html',
  styleUrls: ['./privacy-component.scss']
})
export class PrivacyComponent implements OnInit, OnDestroy {
  constructor(private titleService: TitleService) {}

  ngOnInit() {
    this.titleService.setTitle('Privacy');
  }

  ngOnDestroy() {
    this.titleService.setBaseTitle();
  }
}
