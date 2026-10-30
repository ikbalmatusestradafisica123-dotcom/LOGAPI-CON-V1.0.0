import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { DeliveryEvidence } from '../models/delivery-evidence.model';

@Injectable({
  providedIn: 'root',
})
export class DeliveryEvidenceService {
  private readonly http = inject(HttpClient);

  private readonly endpoint =
    `${environment.apiBaseUrl}/orders`;

  getAll(orderId: number): Observable<DeliveryEvidence[]> {
    return this.http.get<DeliveryEvidence[]>(
      `${this.endpoint}/${orderId}/evidence`,
    );
  }

  upload(
    orderId: number,
    file: File,
    comments: string,
  ): Observable<DeliveryEvidence> {
    const formData = new FormData();

    formData.append('file', file);
    formData.append('evidenceType', 'Delivery');
    formData.append('comments', comments);

    return this.http.post<DeliveryEvidence>(
      `${this.endpoint}/${orderId}/evidence`,
      formData,
    );
  }

  download(
    orderId: number,
    evidenceId: number,
  ): Observable<Blob> {
    return this.http.get(
      `${this.endpoint}/${orderId}/evidence/` +
      `${evidenceId}/file`,
      {
        responseType: 'blob',
      },
    );
  }
}
