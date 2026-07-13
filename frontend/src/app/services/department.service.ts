import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { DepartmentResponseDto, CreateDepartmentDto, UpdateDepartmentDto, PagedDepartments } from '../models/department.model';

@Injectable({ providedIn: 'root' })
export class DepartmentService {
  private readonly base = (environment.apiUrl?.replace(/\/$/, '') || '/api/v1');

  constructor(private readonly http: HttpClient) {}

  list(skip = 0, take = 50, search = '', sortBy = '', sortDir = ''): Observable<PagedDepartments> {
    let params = new HttpParams().set('skip', skip).set('take', take);
    if (search) params = params.set('search', search);
    if (sortBy) params = params.set('sortBy', sortBy);
    if (sortDir) params = params.set('sortDir', sortDir);
    return this.http.get<PagedDepartments>(`${this.base}/Departments`, { params });
  }

  getById(id: number): Observable<DepartmentResponseDto> {
    return this.http.get<DepartmentResponseDto>(`${this.base}/Departments/${id}`);
  }

  create(payload: CreateDepartmentDto): Observable<DepartmentResponseDto> {
    return this.http.post<DepartmentResponseDto>(`${this.base}/Departments`, payload);
  }

  update(id: number, payload: UpdateDepartmentDto): Observable<DepartmentResponseDto> {
    return this.http.put<DepartmentResponseDto>(`${this.base}/Departments/${id}`, payload);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/Departments/${id}`);
  }
}
