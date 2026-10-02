import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { API_BASE_URL } from '../api.config';
import { User } from '../models/user.model';

interface LoginResponse {
  token: string;
  expiresAt: string;
  user: User;
}

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private readonly tokenKey = 'bugboard26-token';
  private readonly userKey = 'bugboard26-user';
  private readonly expiresAtKey = 'bugboard26-expires-at';

  constructor(private readonly http: HttpClient) {
  }

  login(email: string, password: string): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${API_BASE_URL}/auth/login`, { email, password })
      .pipe(
        tap(response => {
          localStorage.setItem(this.tokenKey, response.token);
          localStorage.setItem(this.userKey, JSON.stringify(response.user));
          localStorage.setItem(this.expiresAtKey, response.expiresAt);
        })
      );
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    localStorage.removeItem(this.userKey);
    localStorage.removeItem(this.expiresAtKey);
  }

  getToken(): string | null {
    return this.getSession()?.token ?? null;
  }

  isLoggedIn(): boolean {
    return this.getToken() !== null;
  }

  getCurrentUser(): User | null {
    return this.getSession()?.user ?? null;
  }

  private getSession(): LoginResponse | null {
    const token = localStorage.getItem(this.tokenKey);
    const storedUser = localStorage.getItem(this.userKey);
    const expiresAt = localStorage.getItem(this.expiresAtKey);

    if (!token?.trim() || !storedUser || !expiresAt ||
        !Number.isFinite(Date.parse(expiresAt)) || Date.parse(expiresAt) <= Date.now()) {
      this.logout();
      return null;
    }

    try {
      const user: User | null = JSON.parse(storedUser);

      if (!user || !Number.isInteger(user.id) || user.id <= 0 ||
          typeof user.email !== 'string' || !user.email.trim() ||
          !['ADMIN', 'USER', 'READONLY'].includes(user.role) || user.isActive !== true) {
        this.logout();
        return null;
      }

      return { token, user, expiresAt };
    } catch {
      this.logout();
      return null;
    }
  }
}
