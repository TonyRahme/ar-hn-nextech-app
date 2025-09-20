import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BehaviorSubject, finalize } from 'rxjs';
import { ItemDto, PagedResult } from 'src/app/models/hackerNews.model';
import { HnApiService } from 'src/app/services/hn-api.service';
import { MatButtonModule }  from '@angular/material/button';
import { MatCardModule }    from '@angular/material/card';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-hn-newest-page',
  standalone: true,
  imports: [CommonModule, MatButtonModule, MatCardModule, MatPaginatorModule, MatIconModule],
  templateUrl: './hn-newest-page.component.html',
  styleUrls: ['./hn-newest-page.component.scss']
})
export class HnNewestPageComponent implements OnInit {

  pageSizeOptions: number[] = [10, 20, 30, 50];
  page: number = 1;
  pageSize: number = 20;
  data$ = new BehaviorSubject<PagedResult<ItemDto> | null>(null);
  loading$ = new BehaviorSubject<boolean>(false);
  error$ = new BehaviorSubject<string | null>(null);

  constructor(private hnApi: HnApiService) {}


  ngOnInit() {
    this.load();
  }

  private load() {
    this.loading$.next(true);
    this.error$.next(null);

    this.hnApi.getNewestPage(this.page, this.pageSize)
    .pipe(finalize(() => this.loading$.next(false)))
    .subscribe({
      next: (result) => this.data$.next(result),
      error: (err) => this.error$.next(err?.message ?? "Request Failed")
    });
  }

  next() {
    if (this.data$.value?.hasNext) {
      this.page += 1;
      this.load();
    }
  }

  prev() {
    if (this.page > 1) {
      this.page -= 1;
      this.load();
    }
  }

  onPage(e: PageEvent) {
    this.page = e.pageIndex + 1;
    this.pageSize = e.pageSize;
    this.load();
  }

  totalPages(pagedRes: PagedResult<ItemDto> | null) {
    if(!pagedRes) return 1;

    return Math.max(1, Math.ceil(pagedRes.totalCount / pagedRes.pageSize));
  }

  trackId(_: number, it: ItemDto) {return it.id }

  plainText(html?: string) {
    return (html || '').replace(/<[^>]+>/g, '');
  }

  
}
