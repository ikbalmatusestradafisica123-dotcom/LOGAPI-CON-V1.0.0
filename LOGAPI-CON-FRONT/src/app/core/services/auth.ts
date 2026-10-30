import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  LoginRequest,
  LoginResponse,
} from '../models/auth.models';

@Injectable({
  providedIn: 'root',
})
export class Auth {
  private readonly http = inject(HttpClient);

  private readonly tokenStorageKey = 'logapi_access_token';
  private readonly sessionStorageKey = 'logapi_session';

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http
      .post<LoginResponse>(
        `${environment.apiBaseUrl}/Auth/login`,
        request,
      )
      .pipe(
        tap((response) => {
          sessionStorage.setItem(
            this.tokenStorageKey,
            response.accessToken,
          );

          sessionStorage.setItem(
            this.sessionStorageKey,
            JSON.stringify(response),
          );
        }),
      );
  }

  getToken(): string | null {
    return sessionStorage.getItem(this.tokenStorageKey);
  }

  getSession(): LoginResponse | null {
    const value = sessionStorage.getItem(
      this.sessionStorageKey,
    );

    if (!value) {
      return null;
    }

    try {
      return JSON.parse(value) as LoginResponse;
    } catch {
      this.logout();
      return null;
    }
  }

  isAuthenticated(): boolean {
    const token = this.getToken();

    if (!token) {
      return false;
    }

    const session = this.getSession();

    if (!session) {
      return false;
    }

    const expiration = new Date(
      session.expiresAtUtc,
    ).getTime();

    if (Number.isNaN(expiration)) {
      this.logout();
      return false;
    }

    if (expiration <= Date.now()) {
      this.logout();
      return false;
    }

    return true;
  }

  getRole(): string | null {
    return this.getSession()?.role ?? null;
  }

  logout(): void {
    sessionStorage.removeItem(this.tokenStorageKey);
    sessionStorage.removeItem(this.sessionStorageKey);
  }
}
