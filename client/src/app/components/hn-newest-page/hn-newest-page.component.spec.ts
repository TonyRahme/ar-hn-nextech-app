import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';

import { HnNewestPageComponent } from './hn-newest-page.component';
import { ItemDto, PagedResult } from 'src/app/models/hackerNews.model';
import { HnApiService } from 'src/app/services/hn-api.service';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of, Subject, throwError } from 'rxjs';
import { By } from '@angular/platform-browser';
import { PageEvent } from '@angular/material/paginator';


function makePage(
  items: Partial<ItemDto>[],
  page = 1,
  pageSize = 20,
  totalCount?: number,
  hasNext = false
): PagedResult<ItemDto> {
  return {
    items: items.map(
      (i) =>
        ({
          id: i.id ?? 1,
          time: i.time ?? 0,
          title: i.title,
          url: i.url,
          by: i.by,
          score: i.score,
          kids: i.kids,
          type: i.type,
          text: i.text,
        } as ItemDto),
    ),
    page,
    pageSize,
    totalCount: totalCount ?? items.length,
    hasNext,
  };
}

describe('HnNewestPageComponent', () => {
  let component: HnNewestPageComponent;
  let fixture: ComponentFixture<HnNewestPageComponent>;
  let api: jasmine.SpyObj<HnApiService>;

  beforeEach(async () => {
    api = jasmine.createSpyObj('HnApiService', ['getNewestPage']);
    await TestBed.configureTestingModule({
      imports: [HnNewestPageComponent, NoopAnimationsModule],
      providers: [{provide: HnApiService, useValue: api}],
    }).compileComponents();
    fixture = TestBed.createComponent(HnNewestPageComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('loads first page on init and renders cards (link + plain title)', () => {

    //Arrange
    const page = makePage([
      {id: 1, title: 'A', url: 'https://a' },
      {id: 2, title: 'B' },
    ], 1, 20, 2, false);

    api.getNewestPage.and.returnValue(of(page));
    fixture.detectChanges();

    //Assert
    expect(api.getNewestPage).toHaveBeenCalledWith(1, 20);

    const cards = fixture.debugElement.queryAll(By.css('mat-card.story-card'));
    expect(cards.length).toBe(2);

    const firstLink = cards[0].query(By.css('.title a'));
    expect(firstLink).toBeTruthy();
    expect(firstLink.nativeElement.textContent).toContain('A');

    const secondSpan = cards[1].query(By.css('.title span'));
    expect(secondSpan).toBeTruthy();
    expect(secondSpan.nativeElement.textContent).toContain('B');
  });

  it('paginator: moving to next page calls sevice with new page & size', () => {
    const page = makePage([{id: 1, title: 'A' },], 1, 20, 40, true);
    api.getNewestPage.and.returnValue(of(page));
    fixture.detectChanges();

    api.getNewestPage.calls.reset();

    //simulate paginator event
    component.onPage({
      pageIndex: 1, //page 2 as pageIndex is zero-based
      pageSize: 20,
      length: 40,
      previousPageIndex: 0,
    } as PageEvent);

    expect(api.getNewestPage).toHaveBeenCalledWith(2, 20);
  });

  it('renders error message when service fails', () => {
    api.getNewestPage.and.returnValue(throwError(() => new Error("skibidi error")));
    fixture.detectChanges();

    const err = fixture.debugElement.query(By.css('.error'));
    expect(err).toBeTruthy();
    expect(err.nativeElement.textContent).toContain("skibidi error");
  })

  it('disables Next when hasNext=false', () => {
    const page = makePage([{ id: 1 }], 1, 20, 1, false);
    api.getNewestPage.and.returnValue(of(page));
    fixture.detectChanges();

    expect(component.data$.value?.hasNext).toBeFalse();
  });

  it('rapid Next cancels previous fetch and latest result wins', fakeAsync(() => {
    const slow$ = new Subject<PagedResult<ItemDto>>();
    api.getNewestPage.and.returnValue(slow$);
    fixture.detectChanges();

    const fastPage = makePage([{id: 32, title: 'page 2'}], 2, 20, 40, true);
    api.getNewestPage.calls.reset();
    api.getNewestPage.and.returnValue(of(fastPage));

    const evt: PageEvent = {
      pageIndex: 1,
      pageSize: 20,
      length: 40,
      previousPageIndex: 0
    };
    component.onPage(evt);
    fixture.detectChanges();

    const slowPage = makePage([{ id: 11, title: 'Page 1 (late)' }], 1, 20, 40, true);
    slow$.next(slowPage);
    slow$.complete();
    tick(); //flush microtasks

    expect(component.data$.value).toEqual(fastPage);
  }))

});
