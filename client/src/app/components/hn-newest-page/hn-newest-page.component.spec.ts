import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HnNewestPageComponent } from './hn-newest-page.component';

describe('HnNewestPageComponent', () => {
  let component: HnNewestPageComponent;
  let fixture: ComponentFixture<HnNewestPageComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HnNewestPageComponent]
    });
    fixture = TestBed.createComponent(HnNewestPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
