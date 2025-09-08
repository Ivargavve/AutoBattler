import { Component, OnInit, OnDestroy } from '@angular/core';
import { TitleService } from '../../services/title.service';

@Component({
  selector: 'app-tales-component',
  imports: [],
  templateUrl: './tales-component.html',
  styleUrl: './tales-component.scss'
})
export class TalesComponent implements OnInit, OnDestroy {
  constructor(private titleService: TitleService) {}

  ngOnInit() {
    this.titleService.setTitle('Tales');
  }

  ngOnDestroy() {
    this.titleService.setBaseTitle();
  }
}
