export interface ChangeOrderStatusRequest {
  newStatus: number;
  reason: string;
  ticketReference: string | null;
  isRollback: boolean;
}


export interface DeliveryOrder {
  id: number;
  externalFolio: string | null;
  customer: string;
  weight: number;
  deliveryAddress: string;
  sector: string;
  createdAt: string;
  scheduledDate: string | null;
  status: number;
  isPriority: boolean;
  priorityReason: string | null;
}

export interface CreateDeliveryOrderRequest {
  customer: string;
  weight: number;
  deliveryAddress: string;
  sector: string;
  scheduledDate: string | null;
}

export const orderStatusLabels: Record<number, string> = {
  1: 'Creado',
  2: 'Pendiente de validación',
  3: 'Planeado',
  4: 'En preparación',
  5: 'Asignado',
  6: 'Listo para cargar',
  7: 'Cargado',
  8: 'En ruta',
  9: 'Entregado',
  10: 'Reagendado',
  11: 'Cancelado',
  12: 'Devuelto',
  13: 'Especial',
  14: 'Validado',
  15: 'Pendiente de validación por inventario',
  16: 'Validado por inventario',
  17: 'Pendiente de validación por logística',
  18: 'Validado por Logística',
  19: 'Pendiente de ruteo',
  20: 'Ruteado',
  21: 'Validado por el supervisor de logística',
  22: 'Entrega parcial',
  23: 'Validado por el cliente',
  24: 'Finalizado',
};
