import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HnSearchComponent } from "../hn-search/hn-search.component";
import { HnNewestPageComponent } from "../hn-newest-page/hn-newest-page.component";

@Component({
  selector: 'app-hn-page',
  standalone: true,
  imports: [CommonModule, HnSearchComponent, HnNewestPageComponent],
  templateUrl: './hn-page.component.html',
  styleUrls: ['./hn-page.component.scss']
})
export class HnPageComponent {

}
