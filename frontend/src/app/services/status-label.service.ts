import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { StatusLabelResponseDto, CreateStatusLabelDto, UpdateStatusLabelDto, PagedStatusLabels } from '../models/status-label.model';

@Injectable({ providedIn: 'root' })
export class StatusLabelService {
  private readonly base = (environment.apiUrl?.replace(/\/$/, '') || '/api/v1');

  constructor(private readonly http: HttpClient) {}

  list(skip = 0, take = 50, search = '', sortBy = '', sortDir = ''): Observable<PagedStatusLabels> {
    let params = new HttpParams().set('skip', skip).set('take', take);
    if (search) params = params.set('search', search);
    if (sortBy) params = params.set('sortBy', sortBy);
    if (sortDir) params = params.set('sortDir', sortDir);
    return this.http.get<PagedStatusLabels>(`${this.base}/StatusLabels`, { params });
  }

  getById(id: number): Observable<StatusLabelResponseDto> {
    return this.http.get<StatusLabelResponseDto>(`${this.base}/StatusLabels/${id}`);
  }

  create(payload: CreateStatusLabelDto): Observable<StatusLabelResponseDto> {
    return this.http.post<StatusLabelResponseDto>(`${this.base}/StatusLabels`, payload);
  }

  update(id: number, payload: UpdateStatusLabelDto): Observable<StatusLabelResponseDto> {
    return this.http.put<StatusLabelResponseDto>(`${this.base}/StatusLabels/${id}`, payload);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/StatusLabels/${id}`);
  }
}
