import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { LocationResponseDto, CreateLocationDto, UpdateLocationDto, PagedLocations } from '../models/location.model';

@Injectable({ providedIn: 'root' })
export class LocationService {
  private readonly base = (environment.apiUrl?.replace(/\/$/, '') || '/api/v1');

  constructor(private readonly http: HttpClient) {}

  list(skip = 0, take = 50, search = '', sortBy = '', sortDir = ''): Observable<PagedLocations> {
    let params = new HttpParams().set('skip', skip).set('take', take);
    if (search) params = params.set('search', search);
    if (sortBy) params = params.set('sortBy', sortBy);
    if (sortDir) params = params.set('sortDir', sortDir);
    return this.http.get<PagedLocations>(`${this.base}/Locations`, { params });
  }

  getById(id: number): Observable<LocationResponseDto> {
    return this.http.get<LocationResponseDto>(`${this.base}/Locations/${id}`);
  }

  create(payload: CreateLocationDto): Observable<LocationResponseDto> {
    return this.http.post<LocationResponseDto>(`${this.base}/Locations`, payload);
  }

  update(id: number, payload: UpdateLocationDto): Observable<LocationResponseDto> {
    return this.http.put<LocationResponseDto>(`${this.base}/Locations/${id}`, payload);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/Locations/${id}`);
  }
}
