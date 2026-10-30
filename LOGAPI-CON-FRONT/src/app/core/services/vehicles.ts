import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { CreateVehicleRequest, UpdateVehicleRequest, Vehicle, } from '../models/vehicles.model';

@Injectable({
  providedIn: 'root',
})
export class VehiclesService {
  private readonly http = inject(HttpClient);

  private readonly endpoint =
    `${environment.apiBaseUrl}/Vehicles`;

  getAll(): Observable<Vehicle[]> {
    return this.http.get<Vehicle[]>(
      this.endpoint,
    );
  }

  getAvailable(): Observable<Vehicle[]> {
    return this.http.get<Vehicle[]>(
      `${this.endpoint}/Available`,
    );
  }

  create(
    request: CreateVehicleRequest,
  ): Observable<Vehicle> {
    return this.http.post<Vehicle>(
      this.endpoint,
      request,
    );
  }

  update(
    id: number,
    request: UpdateVehicleRequest,
  ): Observable<Vehicle> {
    return this.http.put<Vehicle>(
      `${this.endpoint}/${id}`,
      request,
    );
  }
}
