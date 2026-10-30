import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { environment } from '../../../environments/environment';
import { CreatePartialDeliveryRequest, PartialDelivery, } from '../models/partial-deliveries.model';

@Injectable({
  providedIn: 'root',
})
export class PartialDeliveriesService {
  private readonly http = inject(HttpClient);

  private readonly baseEndpoint =
    `${environment.apiBaseUrl}/orders`;

  getAll(orderId: number): Observable<PartialDelivery[]> {
    return this.http.get<PartialDelivery[]>(
      `${this.baseEndpoint}/${orderId}/partial-deliveries`,
    );
  }

  create(
    orderId: number,
    request: CreatePartialDeliveryRequest,
  ): Observable<PartialDelivery> {
    return this.http.post<PartialDelivery>(
      `${this.baseEndpoint}/${orderId}/partial-deliveries`,
      request,
    );
  }
}
