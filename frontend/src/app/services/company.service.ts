import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { CompanyResponseDto, CreateCompanyDto, UpdateCompanyDto, PagedCompanies } from '../models/company.model';

@Injectable({ providedIn: 'root' })
export class CompanyService {
  private readonly base = (environment.apiUrl?.replace(/\/$/, '') || '/api/v1');

  constructor(private readonly http: HttpClient) {}

  list(skip = 0, take = 50, search = '', sortBy = '', sortDir = ''): Observable<PagedCompanies> {
    let params = new HttpParams().set('skip', skip).set('take', take);
    if (search) params = params.set('search', search);
    if (sortBy) params = params.set('sortBy', sortBy);
    if (sortDir) params = params.set('sortDir', sortDir);
    return this.http.get<PagedCompanies>(`${this.base}/Companies`, { params });
  }

  getById(id: number): Observable<CompanyResponseDto> {
    return this.http.get<CompanyResponseDto>(`${this.base}/Companies/${id}`);
  }

  create(payload: CreateCompanyDto): Observable<CompanyResponseDto> {
    return this.http.post<CompanyResponseDto>(`${this.base}/Companies`, payload);
  }

  update(id: number, payload: UpdateCompanyDto): Observable<CompanyResponseDto> {
    return this.http.put<CompanyResponseDto>(`${this.base}/Companies/${id}`, payload);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/Companies/${id}`);
  }
}
