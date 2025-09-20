import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HnPageComponent } from './hn-page.component';

describe('HnPageComponent', () => {
  let component: HnPageComponent;
  let fixture: ComponentFixture<HnPageComponent>;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HnPageComponent]
    });
    fixture = TestBed.createComponent(HnPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
