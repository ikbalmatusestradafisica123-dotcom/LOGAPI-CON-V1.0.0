export interface LoadOrder {
  id: number;
  loadId: number;
  orderId: number;
  sequence: number;
  assignedWeight: number;
}

export interface Load {
  id: number;
  vehicleId: number;
  routeDate: string;
  status: number;
  totalWeight: number;
  createdAtUtc: string;
  createdByUserId: string;
  orders: LoadOrder[];
}

export interface AssignLoadRequest {
  routeDate: string;
  vehicleId: number;
  orderIds: number[];
}
