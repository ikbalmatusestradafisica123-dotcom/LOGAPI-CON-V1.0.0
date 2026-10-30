export interface PartialDelivery {
  id: number;
  orderId: number;
  orderedWeight: number;
  deliveredWeight: number;
  pendingWeight: number;
  reason: string;
  comments: string | null;
  rescheduledDate: string | null;
  registeredByUserId: string;
  registeredAtUtc: string;
}

export interface CreatePartialDeliveryRequest {
  deliveredWeight: number;
  reason: string;
  comments: string | null;
  rescheduledDate: string;
}
