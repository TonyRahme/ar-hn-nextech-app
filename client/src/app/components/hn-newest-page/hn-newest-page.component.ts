import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { BehaviorSubject, finalize } from 'rxjs';
import { ItemDto, PagedResult } from 'src/app/models/hackerNews.model';
import { HnApiService } from 'src/app/services/hn-api.service';

@Component({
  selector: 'app-hn-newest-page',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './hn-newest-page.component.html',
  styleUrls: ['./hn-newest-page.component.scss']
})
export class HnNewestPageComponent implements OnInit {

  sizes: number[] = [10, 20, 30, 40];
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

  changePageSize(v: number | string) {
    const n = Number(v) || 20;
    this.pageSize = n;
    this.page = 1;
    this.load();
  }

  totalPages(pagedRes: PagedResult<ItemDto> | null) {
    if(!pagedRes) return 1;

    return Math.max(1, Math.ceil(pagedRes.totalCount / pagedRes.pageSize));
  }

  trackId(_: number, it: ItemDto) {return it.id }

  
}
