import { ComponentFixture, fakeAsync, TestBed, tick } from '@angular/core/testing';

import { HnPageComponent } from './hn-page.component';
import { ItemDto, PagedResult } from 'src/app/models/hackerNews.model';
import { EventEmitter } from '@angular/core';
import { HnApiService } from 'src/app/services/hn-api.service';
import { of } from 'rxjs';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

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

class MockHnApiService {
  newestSearch = new EventEmitter<string>();
  getNewestPage = jasmine.createSpy('getNewestPage');
}

describe('HnPageComponent - Integration of HnSearch + HnNewestPage', () => {
  let component: HnPageComponent;
  let fixture: ComponentFixture<HnPageComponent>;
  let api: MockHnApiService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HnPageComponent, NoopAnimationsModule],
      providers: [{ provide: HnApiService, useClass: MockHnApiService }]
    }).compileComponents();

    fixture = TestBed.createComponent(HnPageComponent);
    component = fixture.componentInstance;
    api = TestBed.inject(HnApiService) as unknown as MockHnApiService;
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('typing in search causes newest page to reload', fakeAsync(() => {
    const beforePage = makePage([{id: 1, title: "before"}]);
    api.getNewestPage.and.returnValue(of(beforePage));

    fixture.detectChanges();

    api.getNewestPage.calls.reset();

    const rustPage = makePage([{id: 2, title: "Rust story"}]);
    api.getNewestPage.and.returnValue(of(rustPage));

    api.newestSearch.emit('rust');
    tick(300);

    expect(api.getNewestPage).toHaveBeenCalledWith(1, 20, 'rust');
  }))
});
