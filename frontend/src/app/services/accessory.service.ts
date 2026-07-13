import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface Accessory {
  id: number;
  name: string;
  qty: number;
  locationId?: number;
  assignedUsers?: string;
}

export interface PagedAccessories {
  data: Accessory[];
  skip: number;
  take: number;
  total: number;
}

@Injectable({ providedIn: 'root' })
export class AccessoryService {
  private readonly base = environment.apiUrl?.replace(/\/$/, '') || '/api/v1';

  constructor(private readonly http: HttpClient) {}

  listAccessories(skip = 0, take = 50): Observable<PagedAccessories> {
    const params = new HttpParams()
      .set('skip', String(skip))
      .set('take', String(take));

    return this.http.get<PagedAccessories>(`${this.base}/Accessories`, { params });
  }

  createAccessory(payload: any): Observable<Accessory> {
    return this.http.post<Accessory>(`${this.base}/Accessories`, payload);
  }
}
