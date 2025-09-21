import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { HnApiService } from 'src/app/services/hn-api.service';
@Component({
  selector: 'app-hn-search',
  standalone: true,
  imports: [
    CommonModule,
    MatToolbarModule,
    MatIconModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
  ],
  templateUrl: './hn-search.component.html',
  styleUrls: ['./hn-search.component.scss'],
})
export class HnSearchComponent {
  constructor(private hnApi: HnApiService) {}
  onSearch(term: string) { this.hnApi.newestSearch.emit(term); }
}
