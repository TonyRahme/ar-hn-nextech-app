import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HnSearchComponent } from './hn-search.component';

describe('HnSearchComponent', () => {
  let component: HnSearchComponent;
  let fixture: ComponentFixture<HnSearchComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HnSearchComponent]
    });
    fixture = TestBed.createComponent(HnSearchComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
