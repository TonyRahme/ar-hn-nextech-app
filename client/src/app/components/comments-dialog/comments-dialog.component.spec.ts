import { ComponentFixture, TestBed } from '@angular/core/testing';

import { CommentsDialogComponent } from './comments-dialog.component';
import { HnApiService } from 'src/app/services/hn-api.service';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { ItemDto } from 'src/app/models/hackerNews.model';
import { of } from 'rxjs';
import { By } from '@angular/platform-browser';

describe('CommentsDialogComponent', () => {
  let component: CommentsDialogComponent;
  let fixture: ComponentFixture<CommentsDialogComponent>;
  let api: jasmine.SpyObj<HnApiService>;
  const storyItem: ItemDto = {
      id: 1, title: 'Hello World',
      deleted: false,
      time: 0,
      dead: false
    }
    
    beforeEach(async () => {
      api = jasmine.createSpyObj('HnApiService', ['getCommentsByStoryId']);
      api.getCommentsByStoryId.and.returnValue(of([]));
      await TestBed.configureTestingModule({
        imports: [CommentsDialogComponent, MatDialogModule, NoopAnimationsModule],
        providers: [
          {provide: HnApiService, useValue: api},
          { provide: MAT_DIALOG_DATA, useValue: { story: storyItem } },
          { provide: MatDialogRef, useValue: { close: jasmine.createSpy('close') } },
        ],
      }).compileComponents();
      fixture = TestBed.createComponent(CommentsDialogComponent);
      component = fixture.componentInstance;
    });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('loads comments and renders cards (with deleted comments)', () => {
  
      //Arrange
      const comments: ItemDto[] = [
        {
          id: 2, title: 'A', text: "comment A", by: "author A",
          deleted: false,
          time: 0,
          dead: false
        },
        {
          id: 3, title: 'B', text: "comment B", by: "author B",
          deleted: true,
          time: 0,
          dead: false
        },
      ]
      api.getCommentsByStoryId.and.returnValue(of(comments));
      fixture = TestBed.createComponent(CommentsDialogComponent);
      fixture.detectChanges();
  
      //Assert
      expect(api.getCommentsByStoryId).toHaveBeenCalledWith(storyItem.id);
  
      const cards = fixture.debugElement.queryAll(By.css('mat-card.comment-card'));
      expect(cards.length).toBe(2);
  
      const firstCommentAuthor = cards[0].query(By.css('.comment-author'));
      expect(firstCommentAuthor).toBeTruthy();
      expect(firstCommentAuthor.nativeElement.textContent).toContain(comments[0].by);
  
      const secondCommentAuthor = cards[1].query(By.css('.comment-author'));
      expect(secondCommentAuthor).toBeTruthy();
      expect(secondCommentAuthor.nativeElement.textContent).toContain('[deleted]');
    });
});
