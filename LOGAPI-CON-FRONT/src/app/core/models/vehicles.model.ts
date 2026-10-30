export interface Vehicle {
  id: number;
  name: string;
  capacity: number;
  brand: string;
  model: string;
  licensePlate: string;
  isAvailable: boolean;
}

export interface CreateVehicleRequest {
  name: string;
  capacity: number;
  brand: string;
  model: string;
  licensePlate: string;
  isAvailable: boolean;
}

export interface UpdateVehicleRequest {
  name: string;
  capacity: number;
  brand: string;
  model: string;
  licensePlate: string;
  reason: string;
  isAvailable: boolean;
}
