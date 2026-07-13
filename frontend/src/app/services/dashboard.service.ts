import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

export interface ActionLog {
  id: number;
  actionType: string;
  itemType: string;
  itemId: number;
  targetId?: number;
  targetType?: string;
  createdBy?: number;
  note?: string;
  logMeta?: string;
  actionDate: string;
  createdAt: string;
  adminName?: string;
  targetName?: string;
}

export interface DashboardSummary {
  totalAssets: number;
  totalUsers: number;
  totalLicenses: number;
  totalAccessories: number;
  assetsDeployed: number;
  assetsReadyToDeploy: number;
  assetsPending: number;
  assetsArchived: number;
  recentActivity: ActionLog[];
}

@Injectable({ providedIn: 'root' })
export class DashboardService {
  private readonly base = environment.apiUrl?.replace(/\/$/, '') || '/api/v1';

  constructor(private readonly http: HttpClient) {}

  getSummary(): Observable<DashboardSummary> {
    return this.http.get<DashboardSummary>(`${this.base}/Dashboard/summary`);
  }
}
