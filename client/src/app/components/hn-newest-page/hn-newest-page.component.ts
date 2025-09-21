import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  BehaviorSubject,
  catchError,
  debounceTime,
  distinctUntilChanged,
  finalize,
  of,
  switchMap,
  tap,
} from 'rxjs';
import { ItemDto, PagedResult } from 'src/app/models/hackerNews.model';
import { HnApiService } from 'src/app/services/hn-api.service';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatIconModule } from '@angular/material/icon';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import {MatBadgeModule} from '@angular/material/badge';

type PageParam = {
  page: number;
  pageSize: number;
  search?: string;
};

@Component({
  selector: 'app-hn-newest-page',
  standalone: true,
  imports: [
    CommonModule,
    MatButtonModule,
    MatCardModule,
    MatPaginatorModule,
    MatIconModule,
    MatProgressSpinnerModule,
    MatBadgeModule
  ],
  templateUrl: './hn-newest-page.component.html',
  styleUrls: ['./hn-newest-page.component.scss'],
})
export class HnNewestPageComponent implements OnInit {
  private params$ = new BehaviorSubject<PageParam>({
    page: 1,
    pageSize: 20
  });

  pageSizeOptions: number[] = [10, 20, 30, 50];
  page: number = 1;
  pageSize: number = 20;
  data$ = new BehaviorSubject<PagedResult<ItemDto> | null>(null);
  loading$ = new BehaviorSubject<boolean>(false);
  error$ = new BehaviorSubject<string | null>(null);

  constructor(private hnApi: HnApiService) {}

  ngOnInit() {
    this.hnApi.newestSearch
      .pipe(debounceTime(300), distinctUntilChanged())
      .subscribe((token) => {
        this.page = 1;
        this.params$.next({
          page: this.page,
          pageSize: this.pageSize,
          search: token,
        });
      });
      
    this.params$
      .pipe(
        distinctUntilChanged(
          (a, b) =>
            a.page === b.page &&
            a.pageSize === b.pageSize &&
            (a.search ?? '') === (b.search ?? '')
        ),
        tap(() => {
          this.loading$.next(true);
          this.data$.next(null);
          this.error$.next(null);
        }),
        switchMap((p) =>
          this.hnApi.getNewestPage(p.page, p.pageSize, p.search).pipe(
            catchError((err) => {
              this.error$.next(err?.message ?? 'Request failed');
              // still emit null to keep stream alive
              return of(null as PagedResult<ItemDto> | null);
            }),
            finalize(() => this.loading$.next(false))
          )
        )
      ).subscribe((res) => {
        if (res) this.data$.next(res);
      });
    
      this.emitParams();
  }

  
  onPage(e: PageEvent) {
    this.page = e.pageIndex + 1; //pageIndex is zero-based
    this.pageSize = e.pageSize;
    this.emitParams();
  }
  
  trackId(_: number, it: ItemDto) {
    return it.id;
  }

  onComments(it: ItemDto) {
    console.log(it);
    
  }
  
  
  //#region HELPER FUNCTIONS
  
  private emitParams() {
    const search = this.params$.value.search ?? '';
    this.params$.next({
      page: this.page,
      pageSize: this.pageSize,
      search: search,
    });
  }
  //#endregion
}
