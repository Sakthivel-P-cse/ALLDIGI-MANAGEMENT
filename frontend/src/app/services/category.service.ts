import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CategoryResponseDto, CreateCategoryDto, UpdateCategoryDto, PagedCategories } from '../models/category.model';

@Injectable({ providedIn: 'root' })
export class CategoryService {
  private readonly base = (environment.apiUrl?.replace(/\/$/, '') || '/api/v1');

  constructor(private readonly http: HttpClient) {}

  list(skip = 0, take = 50, search = '', sortBy = '', sortDir = ''): Observable<PagedCategories> {
    let params = new HttpParams().set('skip', skip).set('take', take);
    if (search) params = params.set('search', search);
    if (sortBy) params = params.set('sortBy', sortBy);
    if (sortDir) params = params.set('sortDir', sortDir);
    return this.http.get<PagedCategories>(`${this.base}/Categories`, { params });
  }

  getById(id: number): Observable<CategoryResponseDto> {
    return this.http.get<CategoryResponseDto>(`${this.base}/Categories/${id}`);
  }

  create(payload: CreateCategoryDto): Observable<CategoryResponseDto> {
    return this.http.post<CategoryResponseDto>(`${this.base}/Categories`, payload);
  }

  update(id: number, payload: UpdateCategoryDto): Observable<CategoryResponseDto> {
    return this.http.put<CategoryResponseDto>(`${this.base}/Categories/${id}`, payload);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/Categories/${id}`);
  }
}
