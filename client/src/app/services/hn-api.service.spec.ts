import { TestBed } from '@angular/core/testing';

import { HnApiService } from './hn-api.service';

describe('HnApiService', () => {
  let service: HnApiService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(HnApiService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
