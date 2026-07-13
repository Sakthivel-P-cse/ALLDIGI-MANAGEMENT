import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { ManufacturerResponseDto, CreateManufacturerDto, UpdateManufacturerDto, PagedManufacturers } from '../models/manufacturer.model';

@Injectable({ providedIn: 'root' })
export class ManufacturerService {
  private readonly base = (environment.apiUrl?.replace(/\/$/, '') || '/api/v1');

  constructor(private readonly http: HttpClient) {}

  list(skip = 0, take = 50, search = '', sortBy = '', sortDir = ''): Observable<PagedManufacturers> {
    let params = new HttpParams().set('skip', skip).set('take', take);
    if (search) params = params.set('search', search);
    if (sortBy) params = params.set('sortBy', sortBy);
    if (sortDir) params = params.set('sortDir', sortDir);
    return this.http.get<PagedManufacturers>(`${this.base}/Manufacturers`, { params });
  }

  getById(id: number): Observable<ManufacturerResponseDto> {
    return this.http.get<ManufacturerResponseDto>(`${this.base}/Manufacturers/${id}`);
  }

  create(payload: CreateManufacturerDto): Observable<ManufacturerResponseDto> {
    return this.http.post<ManufacturerResponseDto>(`${this.base}/Manufacturers`, payload);
  }

  update(id: number, payload: UpdateManufacturerDto): Observable<ManufacturerResponseDto> {
    return this.http.put<ManufacturerResponseDto>(`${this.base}/Manufacturers/${id}`, payload);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/Manufacturers/${id}`);
  }
}
