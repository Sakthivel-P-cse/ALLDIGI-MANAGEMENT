import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { environment } from '../../environments/environment';

interface LoginResponse {
  token: string;
  user: { id: number; username: string };
}

const TOKEN_KEY = 'snipeit_token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly base = environment.apiUrl.replace(/\/$/, '') || '/api/v1';
  private userSubject = new BehaviorSubject<{ id: number; username: string } | null>(this.getStoredUser());

  constructor(private http: HttpClient) {}

  login(username: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${this.base.replace(/\/api\/v1$/, '')}/api/auth/login`, { username, password }).pipe(
      tap((res) => {
        this.setToken(res.token);
        this.userSubject.next(res.user);
      })
    );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    this.userSubject.next(null);
  }

  setToken(token: string) {
    localStorage.setItem(TOKEN_KEY, token);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  isAuthenticated(): boolean {
    return !!this.getToken();
  }

  currentUser() {
    return this.userSubject.asObservable();
  }

  isAdmin(): boolean {
    const token = this.getToken();
    if (!token) return false;
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.role === 'admin' || payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] === 'admin';
    } catch {
      return false;
    }
  }

  getUsername(): string {
    const token = this.getToken();
    if (!token) return '';
    try {
      const payload = JSON.parse(atob(token.split('.')[1]));
      return payload.unique_name || payload.sub || '';
    } catch {
      return '';
    }
  }

  private getStoredUser(): { id: number; username: string } | null {
    // Not storing user object persistently for simplicity; return null
    return null;
  }
}
