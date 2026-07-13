import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface License {
  id: number;
  name: string;
  serial?: string;
  seats: number;
  purchaseCost?: number;
  assignedUsers?: string;
}

export interface PagedLicenses {
  data: License[];
  skip: number;
  take: number;
  total: number;
}

@Injectable({ providedIn: 'root' })
export class LicenseService {
  private readonly base = environment.apiUrl?.replace(/\/$/, '') || '/api/v1';

  constructor(private readonly http: HttpClient) {}

  listLicenses(skip = 0, take = 50): Observable<PagedLicenses> {
    const params = new HttpParams()
      .set('skip', String(skip))
      .set('take', String(take));

    return this.http.get<PagedLicenses>(`${this.base}/Licenses`, { params });
  }

  createLicense(payload: any): Observable<License> {
    return this.http.post<License>(`${this.base}/Licenses`, payload);
  }
}
