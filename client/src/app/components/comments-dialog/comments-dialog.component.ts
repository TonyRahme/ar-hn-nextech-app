import { Component, Inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HnApiService } from 'src/app/services/hn-api.service';
import { catchError, Observable, of, shareReplay } from 'rxjs';
import { ItemDto } from 'src/app/models/hackerNews.model';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatCardModule } from '@angular/material/card';

@Component({
  selector: 'app-comments-dialog',
  standalone: true,
  imports: [
    CommonModule,
    MatDialogModule,
    MatProgressSpinnerModule,
    MatCardModule,
  ],
  templateUrl: './comments-dialog.component.html',
  styleUrls: ['./comments-dialog.component.scss'],
})
export class CommentsDialogComponent {
  error?: unknown;
  storyItem!: ItemDto;
  comment$!: Observable<ItemDto[]>;
  constructor(
    private hnApi: HnApiService,
    @Inject(MAT_DIALOG_DATA) public data: { story: ItemDto }
  ) {
    this.storyItem = data.story;
    this.comment$ = this.hnApi.getCommentsByStoryId(this.storyItem.id).pipe(
      catchError((err) => {
        this.error = err;
        return of([] as ItemDto[]);
      }),
      shareReplay(1)
    );
  }
}
