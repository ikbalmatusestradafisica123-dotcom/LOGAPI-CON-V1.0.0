export interface PlanRouteRequest {
  date: string;
  maxTonnagePerDay: number;
}

export interface RouteStop {
  sequence: number;
  orderId: number;
  customer: string;
  deliveryAddress: string;
  weight: number;
}

export interface RoutePlan {
  vehicleId: number;
  vehicleName: string;
  vehicleCapacity: number;
  totalWeight: number;
  remainingCapacity: number;
  distance: number;
  duration: number;
  stops: RouteStop[];
}

export interface PlanRouteResponse {
  initialDate: string;
  routePlans: RoutePlan[];
  orders: unknown[];
  unassignedOrders: unknown[];
}
