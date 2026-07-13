import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ModelResponseDto, CreateModelDto, UpdateModelDto, PagedModels } from '../models/model.model';

@Injectable({ providedIn: 'root' })
export class ModelService {
  private readonly base = (environment.apiUrl?.replace(/\/$/, '') || '/api/v1');

  constructor(private readonly http: HttpClient) {}

  list(skip = 0, take = 50, search = '', sortBy = '', sortDir = ''): Observable<PagedModels> {
    let params = new HttpParams().set('skip', skip).set('take', take);
    if (search) params = params.set('search', search);
    if (sortBy) params = params.set('sortBy', sortBy);
    if (sortDir) params = params.set('sortDir', sortDir);
    return this.http.get<PagedModels>(`${this.base}/Models`, { params });
  }

  getById(id: number): Observable<ModelResponseDto> {
    return this.http.get<ModelResponseDto>(`${this.base}/Models/${id}`);
  }

  create(payload: CreateModelDto): Observable<ModelResponseDto> {
    return this.http.post<ModelResponseDto>(`${this.base}/Models`, payload);
  }

  update(id: number, payload: UpdateModelDto): Observable<ModelResponseDto> {
    return this.http.put<ModelResponseDto>(`${this.base}/Models/${id}`, payload);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/Models/${id}`);
  }
}
