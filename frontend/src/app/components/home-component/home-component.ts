import { Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TitleService } from '../../services/title.service';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './home-component.html',
  styleUrl: './home-component.scss'
})
export class HomeComponent implements OnInit, OnDestroy {
  constructor(private titleService: TitleService) {}

  ngOnInit() {
    this.titleService.setTitle('Home');
  }

  ngOnDestroy() {
    this.titleService.setBaseTitle();
  }
}
