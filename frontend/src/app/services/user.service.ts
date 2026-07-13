import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';
import { PagedResult, PageQuery } from '../models/paged-result.model';
import { apiBase, buildPageParams } from './api-query.util';

export interface User {
  id: number;
  username?: string | null;
  firstName?: string | null;
  lastName?: string | null;
  displayName?: string | null;
  email?: string | null;
  employeeNum?: string | null;
  jobtitle?: string | null;
  phone?: string | null;
  departmentId?: number | null;
  locationId?: number | null;
  companyId?: number | null;
  managerId?: number | null;
  activated: boolean;
  vip?: boolean | null;
  lastLogin?: string | null;
  createdAt?: string | null;
  updatedAt?: string | null;
  departmentName?: string | null;
  locationName?: string | null;
  companyName?: string | null;
}

export interface CreateUserPayload {
  username: string;
  firstName?: string | null;
  lastName?: string | null;
  email?: string | null;
  password: string;
  employeeNum?: string | null;
  jobtitle?: string | null;
  phone?: string | null;
  departmentId?: number | null;
  activated?: boolean;
  vip?: boolean | null;
}

export interface UpdateUserPayload {
  username?: string | null;
  firstName?: string | null;
  lastName?: string | null;
  email?: string | null;
  employeeNum?: string | null;
  jobtitle?: string | null;
  phone?: string | null;
  departmentId?: number | null;
  vip?: boolean | null;
}

export interface AssignedItems {
  assets: { id: number; name?: string; assetTag?: string; serial?: string }[];
  licenses: { id: number; name?: string; serial?: string }[];
  accessories: { id: number; name?: string }[];
}

@Injectable({ providedIn: 'root' })
export class UserService {
  private readonly base = apiBase(environment.apiUrl);

  constructor(private readonly http: HttpClient) {}

  listUsers(query: PageQuery = {}): Observable<PagedResult<User>> {
    return this.http.get<PagedResult<User>>(`${this.base}/Users`, { params: buildPageParams(query) });
  }

  getUserById(id: number): Observable<User> {
    return this.http.get<User>(`${this.base}/Users/${id}`);
  }

  createUser(payload: CreateUserPayload): Observable<User> {
    return this.http.post<User>(`${this.base}/Users`, payload);
  }

  updateUser(id: number, payload: UpdateUserPayload): Observable<User> {
    return this.http.put<User>(`${this.base}/Users/${id}`, payload);
  }

  deleteUser(id: number): Observable<void> {
    return this.http.delete<void>(`${this.base}/Users/${id}`);
  }

  changePassword(id: number, newPassword: string, confirmPassword: string): Observable<void> {
    return this.http.post<void>(`${this.base}/Users/${id}/change-password`, { newPassword, confirmPassword });
  }

  resetPassword(id: number, newPassword: string, confirmPassword: string): Observable<void> {
    return this.http.post<void>(`${this.base}/Users/${id}/reset-password`, { newPassword, confirmPassword });
  }

  setActivation(id: number, activated: boolean): Observable<User> {
    return this.http.patch<User>(`${this.base}/Users/${id}/activation`, { activated });
  }

  getUserItems(id: number): Observable<AssignedItems> {
    return this.http.get<AssignedItems>(`${this.base}/Users/${id}/items`);
  }
}
