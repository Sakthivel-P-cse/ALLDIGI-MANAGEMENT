import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { SupplierResponseDto, CreateSupplierDto, UpdateSupplierDto, PagedSuppliers } from '../models/supplier.model';

@Injectable({ providedIn: 'root' })
export class SupplierService {
  private readonly base = (environment.apiUrl?.replace(/\/$/, '') || '/api/v1');

  constructor(private readonly http: HttpClient) {}

  list(skip = 0, take = 50, search = '', sortBy = '', sortDir = ''): Observable<PagedSuppliers> {
    let params = new HttpParams().set('skip', skip).set('take', take);
    if (search) params = params.set('search', search);
    if (sortBy) params = params.set('sortBy', sortBy);
    if (sortDir) params = params.set('sortDir', sortDir);
    return this.http.get<PagedSuppliers>(`${this.base}/Suppliers`, { params });
  }

  getById(id: number): Observable<SupplierResponseDto> {
    return this.http.get<SupplierResponseDto>(`${this.base}/Suppliers/${id}`);
  }

  create(payload: CreateSupplierDto): Observable<SupplierResponseDto> {
    return this.http.post<SupplierResponseDto>(`${this.base}/Suppliers`, payload);
  }

  update(id: number, payload: UpdateSupplierDto): Observable<SupplierResponseDto> {
    return this.http.put<SupplierResponseDto>(`${this.base}/Suppliers/${id}`, payload);
  }

  delete(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/Suppliers/${id}`);
  }
}
