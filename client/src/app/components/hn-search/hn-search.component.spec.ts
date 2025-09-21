import { ComponentFixture, TestBed } from '@angular/core/testing';

import { HnSearchComponent } from './hn-search.component';
import { HnApiService } from 'src/app/services/hn-api.service';
import { EventEmitter } from '@angular/core';
import { By } from '@angular/platform-browser';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';

describe('HnSearchComponent', () => {
  let component: HnSearchComponent;
  let fixture: ComponentFixture<HnSearchComponent>;
  let api: jasmine.SpyObj<HnApiService>;

  beforeEach(async () => {
    api = jasmine.createSpyObj('HnApiService', ['getNewestPage'],
           {newestSearch: new EventEmitter<string>()}
        );
    await TestBed.configureTestingModule({
      imports: [HnSearchComponent, NoopAnimationsModule],
      providers: [{ provide: HnApiService, useValue: api }]
    }).compileComponents();
    fixture = TestBed.createComponent(HnSearchComponent);
        component = fixture.componentInstance;
      });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('emits newestSearch when input changes and search is triggered', () => {
    spyOn(api.newestSearch, 'emit');

    const input = fixture.debugElement.query(By.css('input')).nativeElement;
    input.value = 'rust';
    input.dispatchEvent(new Event('input'));
    fixture.detectChanges();

    expect(api.newestSearch.emit).toHaveBeenCalledWith('rust');
  });

  
});
