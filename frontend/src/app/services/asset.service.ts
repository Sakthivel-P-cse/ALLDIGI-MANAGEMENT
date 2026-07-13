import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { map } from 'rxjs/operators';
import { AssetResponseDto, CreateAssetDto, UpdateAssetDto, PagedAssets } from '../models/asset.model';
import { environment } from '../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AssetService {
  private readonly base = environment.apiUrl?.replace(/\/$/, '') || '/api/v1';

  constructor(private readonly http: HttpClient) {}

  listAssets(skip = 0, take = 50): Observable<PagedAssets> {
    const params = new HttpParams()
      .set('skip', String(skip))
      .set('take', String(take));

    return this.http.get<PagedAssets>(`${this.base}/Assets`, { params });
  }

  getAsset(id: number): Observable<AssetResponseDto> {
    return this.http.get<AssetResponseDto>(`${this.base}/Assets/${id}`);
  }

  getByTag(tag: string): Observable<AssetResponseDto> {
    return this.http.get<AssetResponseDto>(`${this.base}/Assets/tag/${encodeURIComponent(tag)}`);
  }

  createAsset(payload: CreateAssetDto): Observable<AssetResponseDto> {
    return this.http.post<AssetResponseDto>(`${this.base}/Assets`, payload);
  }

  updateAsset(id: number, payload: UpdateAssetDto): Observable<AssetResponseDto> {
    return this.http.put<AssetResponseDto>(`${this.base}/Assets/${id}`, payload);
  }

  deleteAsset(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/Assets/${id}`);
  }

  checkoutAsset(id: number, userId: number): Observable<AssetResponseDto> {
    return this.http.post<AssetResponseDto>(`${this.base}/Assets/${id}/checkout`, { userId });
  }

  checkinAsset(id: number): Observable<AssetResponseDto> {
    return this.http.post<AssetResponseDto>(`${this.base}/Assets/${id}/checkin`, {});
  }

  getAssetHistory(id: number, skip = 0, take = 50): Observable<any> {
    const params = new HttpParams().set('skip', String(skip)).set('take', String(take));
    return this.http.get<any>(`${this.base}/Assets/${id}/history`, { params });
  }

  exportCsv(): Observable<Blob> {
    return this.http.get(`${this.base}/Assets/export`, { responseType: 'blob' });
  }
}
