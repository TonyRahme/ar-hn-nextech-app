import { TestBed } from '@angular/core/testing';
import {
  HttpClientTestingModule,
  HttpTestingController,
} from '@angular/common/http/testing';

import { HnApiService } from './hn-api.service';
import { ItemDto, PagedResult } from '../models/hackerNews.model';

describe('HnApiService', () => {
  let service: HnApiService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
    imports: [HttpClientTestingModule],
    });
    service = TestBed.inject(HnApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('GETs newest page with defaults', () => {
    let received: PagedResult<ItemDto> | undefined;
    service.getNewestPage().subscribe((e) => received = e);
    const req = httpMock.expectOne(
      (r) => r.method === 'GET' && r.url.endsWith('/api/hackernews/newest/page')
    );

    expect(req.request.params.get('page')).toBe('1');
    expect(req.request.params.get('pageSize')).toBe('20');

    const body: PagedResult<ItemDto> = {
      items: [
        {
          id: 1, time: 0, title: 'A',
          deleted: false,
          dead: false
        },
        {
          id: 2, time: 0, title: 'B',
          deleted: false,
          dead: false
        },
      ],
      page: 1,
      pageSize: 20,
      totalCount: 2,
      hasNext: false,
    };

    req.flush(body);

    expect(received).toBeTruthy();
    expect(received!.items.length).toBe(2);
  });


  it('handles empty items array', () => {
    let received: PagedResult<ItemDto> | undefined;
    service.getNewestPage(3, 10).subscribe((r) => (received = r));

    const req = httpMock.expectOne(
      (r) =>
        r.method === 'GET' &&
      r.url.endsWith('/api/hackernews/newest/page') &&
      r.params.get('page') === '3' &&
      r.params.get('pageSize') === '10'
    );


    const body: PagedResult<ItemDto> = {
      items: [],
      page: 3,
      pageSize: 10,
      totalCount: 0,
      hasNext: false,
    }

    req.flush(body);

    expect(received).toBeTruthy();
    expect(received!.items).toEqual([]);
  });

  it('propagates HTTP errors', () => {
    let error: any;

    service.getNewestPage(2, 50).subscribe({
      next: () => fail("should have errrored"),
      error: (e) => (error = e),
    });

    const req = httpMock.expectOne(
      (r) => r.method === 'GET' && r.url.endsWith('/api/hackernews/newest/page'),
    );

    req.flush({ message: "Ahhh skibidi :/"}, {status: 500, statusText: "Server Error! The server did not serve"});

    expect(error).toBeTruthy();
  })
});
