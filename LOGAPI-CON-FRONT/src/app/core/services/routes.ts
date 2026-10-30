import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { PlanRouteRequest, PlanRouteResponse, } from '../models/routes.model';

@Injectable({
  providedIn: 'root',
})
export class RoutesService {
  private readonly http = inject(HttpClient);

  private readonly endpoint =
    `${environment.apiBaseUrl}/Route`;

  plan(
    request: PlanRouteRequest,
  ): Observable<PlanRouteResponse> {
    return this.http.post<PlanRouteResponse>(
      `${this.endpoint}/plan`,
      request,
    );
  }
}
