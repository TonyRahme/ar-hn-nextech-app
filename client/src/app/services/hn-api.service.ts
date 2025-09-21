import { EventEmitter, Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { ItemDto, PagedResult } from '../models/hackerNews.model';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class HnApiService {

  private baseUrl = '/api/hackernews';
  public newestSearch: EventEmitter<string>;
  constructor(private http: HttpClient) {
    this.newestSearch = new EventEmitter();
   }

  public getNewestPage(page = 1, pageSize = 20, search?: string): Observable<PagedResult<ItemDto>> {
    let params = new HttpParams()
    .set('page', page)
    .set('pageSize', pageSize)
    if(search && search.trim()) params = params.set('search', search.trim());
    
    return this.http.get<PagedResult<ItemDto>>(`${this.baseUrl}/newest/page`, { params });
  }
}
