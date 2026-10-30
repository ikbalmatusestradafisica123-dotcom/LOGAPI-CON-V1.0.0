/// <reference types="google.maps" />
import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, ChangeDetectorRef, AfterViewInit, ElementRef, ViewChild, } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DeliveryOrder, orderStatusLabels, } from '../../../../core/models/orders.model';
import { OrdersService } from '../../../../core/services/orders';
import { forkJoin } from 'rxjs';
import { Load } from '../../../../core/models/loads.model';
import { Vehicle } from '../../../../core/models/vehicles.model';
import { LoadsService } from '../../../../core/services/loads';
import { VehiclesService } from '../../../../core/services/vehicles';
import { GoogleMapsLoader } from '../../../../core/services/google-maps-loader';





@Component({
  selector: 'app-tracking-page',
  imports: [
    CommonModule,
    RouterLink,
  ],
  templateUrl: './tracking-page.html',
  styleUrl: './tracking-page.scss',
})
export class TrackingPage implements OnInit, AfterViewInit {

  private readonly mapsLoader = inject(GoogleMapsLoader);
  private readonly ordersService = inject(OrdersService);
  private readonly loadsService = inject(LoadsService);
  private readonly vehiclesService = inject(VehiclesService);
  private readonly cdr = inject(ChangeDetectorRef);


  orders: DeliveryOrder[] = [];
  trackedOrders: DeliveryOrder[] = [];
  loads: Load[] = [];
  vehicles: Vehicle[] = [];


  isLoading = false;
  errorMessage = '';
  lastUpdated: Date | null = null;

  readonly statusLabels = orderStatusLabels;

  ngOnInit(): void {
    this.loadTracking();
  }

  loadTracking(): void {
    this.isLoading = true;
    this.errorMessage = '';

    forkJoin({
      orders: this.ordersService.getAll(),
      loads: this.loadsService.getAll(),
      vehicles: this.vehiclesService.getAll(),
    }).subscribe({
      next: ({ orders, loads, vehicles }) => {
        this.orders = orders;
        this.loads = loads;
        this.vehicles = vehicles;

        this.trackedOrders = orders
          .filter((order) => order.status >= 7)
          .sort((a, b) => {
            if (
              a.status === 8 &&
              b.status !== 8
            ) {
              return -1;
            }

            if (
              a.status !== 8 &&
              b.status === 8
            ) {
              return 1;
            }

            return b.id - a.id;
          });

        this.lastUpdated = new Date();
        this.isLoading = false;
        this.cdr.detectChanges();
      },

      error: (error: unknown) => {
        this.isLoading = false;

        if (
          error instanceof HttpErrorResponse &&
          error.status === 401
        ) {
          this.errorMessage =
            'Tu sesión expiró. Inicia sesión nuevamente.';
        } else if (
          error instanceof HttpErrorResponse &&
          error.status === 403
        ) {
          this.errorMessage =
            'No tienes permisos para consultar el seguimiento.';
        } else {
          this.errorMessage =
            'No fue posible cargar el seguimiento.';
        }

        this.cdr.detectChanges();
      },
    });
  }

  getVehicleForOrder(
    orderId: number,
  ): Vehicle | null {
    const load = this.loads.find((item) =>
      item.orders.some(
        (loadOrder) =>
          loadOrder.orderId === orderId,
      ),
    );

    if (!load) {
      return null;
    }

    return this.vehicles.find(
      (vehicle) =>
        vehicle.id === load.vehicleId,
    ) ?? null;
  }


  getLoadForOrder(
    orderId: number,
  ): Load | null {
    return this.loads.find((load) =>
      load.orders.some(
        (loadOrder) =>
          loadOrder.orderId === orderId,
      ),
    ) ?? null;
  }



  getStatusLabel(status: number): string {
    return this.statusLabels[status] ??
      `Estado ${status}`;
  }

  getProgress(status: number): number {
    const progressByStatus: Record<number, number> = {
      7: 55,
      8: 70,
      21: 62,
      23: 85,
      9: 95,
      24: 100,
    };

    return progressByStatus[status] ?? 0;
  }

  getProgressLabel(status: number): string {
    if (status === 8) {
      return 'En ruta';
    }

    if (status === 9 || status === 24) {
      return 'Entrega completada';
    }

    return 'Operación en proceso';
  }

  trackByOrderId(
    _index: number,
    order: DeliveryOrder,
  ): number {
    return order.id;
  }



  private async initializeMap(): Promise<void> {
    try {
      await this.mapsLoader.load();

      if (!this.mapContainer) {
        return;
      }

      this.map = new google.maps.Map(
        this.mapContainer.nativeElement,
        {
          center: {
            lat: 19.4326,
            lng: -99.1332,
          },
          zoom: 11,
          mapTypeControl: false,
          streetViewControl: false,
          fullscreenControl: true,
        },
      );
    } catch {
      this.mapError =
        'No se pudo cargar Google Maps. ' +
        'Verifica la clave y sus restricciones.';

      this.cdr.detectChanges();
    }
  }

  ngAfterViewInit(): void {
    this.initializeMap();
  }

  @ViewChild('mapContainer')
  private mapContainer?: ElementRef<HTMLDivElement>;

  mapError = '';
  private map: google.maps.Map | null = null;



  expandedOrderIds = new Set<number>();

  toggleCard(orderId: number): void{
    if (this.expandedOrderIds.has(orderId)) {
      this.expandedOrderIds.delete(orderId);
    }
    else {
      this.expandedOrderIds.add(orderId);
    }
  }

  isExpanded(orderId: number): boolean {
    return this.expandedOrderIds.has(orderId)
  }


}
