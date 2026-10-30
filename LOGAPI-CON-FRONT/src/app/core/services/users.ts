import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import {
  CreateUserRequest,
  UpdateUserRoleRequest,
  UpdateUserStatusRequest,
  UserResponse,
} from '../models/users.model';

@Injectable({
  providedIn: 'root',
})
export class UsersService {
  private readonly http = inject(HttpClient);

  private readonly endpoint =
    `${environment.apiBaseUrl}/Users`;

  getAll(): Observable<UserResponse[]> {
    return this.http.get<UserResponse[]>(
      this.endpoint,
    );
  }

  create(
    request: CreateUserRequest,
  ): Observable<UserResponse> {
    return this.http.post<UserResponse>(
      this.endpoint,
      request,
    );
  }

  updateRole(
    id: string,
    request: UpdateUserRoleRequest,
  ): Observable<UserResponse> {
    return this.http.put<UserResponse>(
      `${this.endpoint}/${id}/role`,
      request,
    );
  }

  updateStatus(
    id: string,
    request: UpdateUserStatusRequest,
  ): Observable<UserResponse> {
    return this.http.patch<UserResponse>(
      `${this.endpoint}/${id}/status`,
      request,
    );
  }
}
