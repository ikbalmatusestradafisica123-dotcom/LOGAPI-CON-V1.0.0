import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ChangeOrderStatusRequest, CreateDeliveryOrderRequest, DeliveryOrder,} from '../models/orders.model';

@Injectable({
  providedIn: 'root',
})
export class OrdersService {
  private readonly http = inject(HttpClient);

  private readonly endpoint =
    `${environment.apiBaseUrl}/Orders`;

  getAll(): Observable<DeliveryOrder[]> {
    return this.http.get<DeliveryOrder[]>(this.endpoint,);
  }

  getById(id: number): Observable<DeliveryOrder> {
    return this.http.get<DeliveryOrder>(`${this.endpoint}/${id}`,);
  }

  create(request: CreateDeliveryOrderRequest,): Observable<DeliveryOrder> {
    return this.http.post<DeliveryOrder>(this.endpoint, request,);
  }

  changeStatus(id: number, request: ChangeOrderStatusRequest,): Observable<DeliveryOrder> {
    return this.http.patch<DeliveryOrder>(`${this.endpoint}/${id}/estatus`, request,);
  }

}
