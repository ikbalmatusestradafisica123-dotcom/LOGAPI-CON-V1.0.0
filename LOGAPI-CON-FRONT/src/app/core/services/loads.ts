import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AssignLoadRequest, Load, } from '../models/loads.model';

@Injectable({
  providedIn: 'root',
})
export class LoadsService {
  private readonly http = inject(HttpClient);

  private readonly endpoint =
    `${environment.apiBaseUrl}/Loads`;

  getAll(): Observable<Load[]> {
    return this.http.get<Load[]>(
      this.endpoint,
    );
  }

  assign(
    request: AssignLoadRequest,
  ): Observable<Load> {
    return this.http.post<Load>(
      `${this.endpoint}/assign`,
      request,
    );
  }
}
