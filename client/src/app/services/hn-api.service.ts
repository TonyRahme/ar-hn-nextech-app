import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { ItemDto, PagedResult } from '../models/hackerNews.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class HnApiService {

  private baseUrl = '/api/hackernews';
  constructor(private http: HttpClient) { }

  public getNewestPage(page = 1, pageSize = 20): Observable<PagedResult<ItemDto>> {
    const params = new HttpParams().set('page', page)
    .set('pageSize', pageSize);

    return this.http.get<PagedResult<ItemDto>>(`${this.baseUrl}/newest/page`, { params });
  }
}
