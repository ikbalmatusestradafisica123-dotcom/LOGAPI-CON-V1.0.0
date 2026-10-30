import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, ChangeDetectorRef,} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { DeliveryOrder,} from '../../../../core/models/orders.model';
import { AssignLoadRequest, } from '../../../../core/models/loads.model';
import { Vehicle } from '../../../../core/models/vehicles.model';
import { OrdersService } from '../../../../core/services/orders';
import { VehiclesService } from '../../../../core/services/vehicles';
import { LoadsService } from '../../../../core/services/loads';

@Component({
  selector: 'app-loads-page',
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
  ],
  templateUrl: './loads-page.html',
  styleUrl: './loads-page.scss',
})
export class LoadsPage implements OnInit {
  private readonly ordersService = inject(OrdersService);
  private readonly vehiclesService = inject(VehiclesService);
  private readonly loadsService = inject(LoadsService);
  private readonly cdr = inject(ChangeDetectorRef); // <--- necesario para obligar a Angular a refrescar la vista

  pendingOrders: DeliveryOrder[] = [];
  availableVehicles: Vehicle[] = [];
  selectedOrders: DeliveryOrder[] = [];

  selectedVehicleId: number | null = null;
  routeDate = new Date()
    .toISOString()
    .slice(0, 10);

  isLoading = false;
  isSaving = false;
  errorMessage = '';
  successMessage = '';

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.ordersService.getAll().subscribe({
      next: (orders) => {
        this.pendingOrders = orders.filter(
          (order) =>
            order.status === 19 ||
            order.status === 20,
        );

        this.loadVehicles();
      },
      error: (error: unknown) => {
        this.isLoading = false;
        this.errorMessage = this.getErrorMessage(
          error,
          'No fue posible cargar los pedidos.',
        );
        this.cdr.detectChanges(); // <--- Refresca la vista si falla
      },
    });
  }

  private loadVehicles(): void {
    this.vehiclesService.getAvailable().subscribe({
      next: (vehicles) => {
        this.availableVehicles = vehicles;
        this.isLoading = false;
        this.cdr.detectChanges(); // <--- Refresca la vista al terminar de cargar todo
      },
      error: (error: unknown) => {
        this.isLoading = false;
        this.errorMessage = this.getErrorMessage(
          error,
          'No fue posible cargar los vehículos.',
        );
        this.cdr.detectChanges(); // <--- Refresca la vista si falla
      },
    });
  }

  isSelected(order: DeliveryOrder): boolean {
    return this.selectedOrders.some(
      (selected) => selected.id === order.id,
    );
  }

  toggleOrder(order: DeliveryOrder): void {
    if (this.isSelected(order)) {
      this.selectedOrders =
        this.selectedOrders.filter(
          (selected) => selected.id !== order.id,
        );

      return;
    }

    this.selectedOrders = [
      ...this.selectedOrders,
      order,
    ];
  }

  moveUp(index: number): void {
    if (index <= 0) {
      return;
    }

    const copy = [...this.selectedOrders];

    [
      copy[index - 1],
      copy[index],
    ] = [
        copy[index],
        copy[index - 1],
      ];

    this.selectedOrders = copy;
  }

  moveDown(index: number): void {
    if (
      index < 0 ||
      index >= this.selectedOrders.length - 1
    ) {
      return;
    }

    const copy = [...this.selectedOrders];

    [
      copy[index],
      copy[index + 1],
    ] = [
        copy[index + 1],
        copy[index],
      ];

    this.selectedOrders = copy;
  }

  removeOrder(index: number): void {
    this.selectedOrders =
      this.selectedOrders.filter(
        (_order, currentIndex) =>
          currentIndex !== index,
      );
  }

  get selectedWeight(): number {
    return this.selectedOrders.reduce(
      (total, order) => total + order.weight,
      0,
    );
  }

  get selectedVehicle(): Vehicle | null {
    return this.availableVehicles.find(
      (vehicle) =>
        vehicle.id === this.selectedVehicleId,
    ) ?? null;
  }

  saveLoad(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (
      !this.routeDate ||
      !this.selectedVehicleId ||
      this.selectedOrders.length === 0
    ) {
      this.errorMessage =
        'Selecciona fecha, vehículo y al menos un pedido.';
      return;
    }

    if (
      this.selectedVehicle &&
      this.selectedWeight >
      this.selectedVehicle.capacity
    ) {
      this.errorMessage =
        'El peso seleccionado supera la capacidad del vehículo.';
      return;
    }

    const request: AssignLoadRequest = {
      routeDate: `${this.routeDate}T00:00:00`,
      vehicleId: this.selectedVehicleId,
      orderIds: this.selectedOrders.map(
        (order) => order.id,
      ),
    };

    this.isSaving = true;

    this.loadsService.assign(request).subscribe({
      next: (load) => {
        this.isSaving = false;
        this.successMessage =
          `Carga #${load.id} guardada correctamente.`;

        const assignedIds = new Set(
          request.orderIds,
        );

        this.pendingOrders =
          this.pendingOrders.filter(
            (order) =>
              !assignedIds.has(order.id),
          );

        this.selectedOrders = [];
        this.selectedVehicleId = null;
        this.cdr.detectChanges(); // <--- Refresca la vista al guardar con éxito
      },
      error: (error: unknown) => {
        this.isSaving = false;
        this.errorMessage = this.getErrorMessage(
          error,
          'No fue posible guardar la carga.',
        );
        this.cdr.detectChanges(); // <--- Refresca la vista en error al guardar
      },
    });
  }

  private getErrorMessage(
    error: unknown,
    fallback: string,
  ): string {
    if (
      error instanceof HttpErrorResponse &&
      error.status === 401
    ) {
      return 'Tu sesión expiró.';
    }

    if (
      error instanceof HttpErrorResponse &&
      error.status === 403
    ) {
      return 'No tienes permisos para asignar cargas.';
    }

    if (
      error instanceof HttpErrorResponse &&
      (
        error.status === 400 ||
        error.status === 409
      )
    ) {
      return error.error?.message ??
        'La asignación no es válida.';
    }

    return fallback;
  }
}
