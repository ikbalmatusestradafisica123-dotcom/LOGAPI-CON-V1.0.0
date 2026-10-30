import { CommonModule } from '@angular/common';
import { Component, OnInit, inject, ChangeDetectorRef, } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { RouterLink } from '@angular/router';
import { ChangeOrderStatusRequest, CreateDeliveryOrderRequest, DeliveryOrder, orderStatusLabels, } from '../../../../core/models/orders.model';
import { OrdersService } from '../../../../core/services/orders';
import { PartialDeliveriesService } from '../../../../core/services/partial-deliveries';
import { CreatePartialDeliveryRequest } from '../../../../core/models/partial-deliveries.model';
import { DeliveryEvidence } from '../../../../core/models/delivery-evidence.model';
import { DeliveryEvidenceService } from '../../../../core/services/delivery-evidence';



@Component({
  selector: 'app-orders-page',
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
  ],
  templateUrl: './orders-page.html',
  styleUrl: './orders-page.scss',
})
export class OrdersPage implements OnInit {
  private readonly ordersService = inject(OrdersService);
  private readonly partialDeliveriesService = inject(PartialDeliveriesService);
  private readonly cdr = inject(ChangeDetectorRef);
  private readonly evidenceService = inject(DeliveryEvidenceService);

  orders: DeliveryOrder[] = [];
  filteredOrders: DeliveryOrder[] = [];

  isLoading = false;
  isCreating = false;
  updatingOrderId: number | null = null;

  errorMessage = '';
  createErrorMessage = '';
  successMessage = '';

  searchTerm = '';
  selectedStatus = '';

  customer = '';
  weight: number | null = null;
  deliveryAddress = '';
  sector = '';
  scheduledDate = '';


  partialOrder: DeliveryOrder | null = null;
  partialDeliveredWeight: number | null = null;
  partialReason = '';
  partialComments = '';
  partialRescheduledDate = '';
  isSavingPartial = false;

  evidenceOrder: DeliveryOrder | null = null;
  evidenceItems: DeliveryEvidence[] = [];
  evidenceFile: File | null = null;
  evidenceComments = '';
  isLoadingEvidence = false;
  isUploadingEvidence = false;



  readonly statusLabels = orderStatusLabels;

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.ordersService.getAll().subscribe({
      next: (orders) => {
        this.orders = orders;
        this.applyFilters();
        this.isLoading = false;
        this.cdr.detectChanges(); 
      },
      error: (error: unknown) => {
        this.isLoading = false;

        if (error instanceof HttpErrorResponse && error.status === 401) {
          this.errorMessage = 'Tu sesión expiró. Inicia sesión nuevamente.';
          this.cdr.detectChanges();
          return;
        }

        if (error instanceof HttpErrorResponse && error.status === 403) {
          this.errorMessage = 'No tienes permisos para consultar pedidos.';
          this.cdr.detectChanges(); 
          return;
        }

        this.errorMessage = 'No fue posible cargar los pedidos.';
        this.cdr.detectChanges(); 
      },
    });
  }

  createOrder(): void {
    this.createErrorMessage = '';
    this.successMessage = '';

    const normalizedCustomer = this.customer.trim();
    const normalizedAddress = this.deliveryAddress.trim();
    const normalizedSector = this.sector.trim();

    if (
      !normalizedCustomer ||
      !normalizedAddress ||
      !normalizedSector ||
      this.weight === null ||
      this.weight <= 0
    ) {
      this.createErrorMessage = 'Completa todos los campos obligatorios.';
      return;
    }

    const request: CreateDeliveryOrderRequest = {
      customer: normalizedCustomer,
      weight: this.weight,
      deliveryAddress: normalizedAddress,
      sector: normalizedSector,
      scheduledDate: this.scheduledDate
        ? `${this.scheduledDate}T00:00:00`
        : null,
    };

    this.isCreating = true;

    this.ordersService.create(request).subscribe({
      next: (createdOrder) => {
        this.isCreating = false;

        this.orders = [
          createdOrder,
          ...this.orders,
        ];

        this.resetCreateForm();
        this.applyFilters();

        this.successMessage =
          `Pedido #${createdOrder.id} creado correctamente.`;
        this.cdr.detectChanges(); 
      },
      error: (error: unknown) => {
        this.isCreating = false;

        if (error instanceof HttpErrorResponse && error.status === 401) {
          this.createErrorMessage = 'Tu sesión expiró. Inicia sesión nuevamente.';
          this.cdr.detectChanges();
          return;
        }

        if (error instanceof HttpErrorResponse && error.status === 403) {
          this.createErrorMessage = 'No tienes permisos para crear pedidos.';
          this.cdr.detectChanges(); 
          return;
        }

        if (error instanceof HttpErrorResponse && error.status === 400) {
          this.createErrorMessage = 'Los datos del pedido no son válidos.';
          this.cdr.detectChanges(); 
          return;
        }

        this.createErrorMessage = 'No fue posible crear el pedido.';
        this.cdr.detectChanges(); 
      },
    });
  }


  validateOrder(order: DeliveryOrder): void {
    if (order.status !== 1) {
      return;
    }

    this.updatingOrderId = order.id;
    this.errorMessage = '';
    this.successMessage = '';

    const request: ChangeOrderStatusRequest = {
      newStatus: 14,
      reason: 'Validación inicial del pedido desde el módulo de pedidos.',
      ticketReference: null,
      isRollback: false,
    };

    this.ordersService
      .changeStatus(order.id, request)
      .subscribe({
        next: (updatedOrder) => {
          this.updatingOrderId = null;

          this.orders = this.orders.map((item) =>
            item.id === updatedOrder.id
              ? updatedOrder
              : item,
          );

          this.applyFilters();

          this.successMessage =
            `El pedido #${updatedOrder.id} fue validado correctamente.`;
          this.cdr.detectChanges(); 
        },
        error: (error: unknown) => {
          this.updatingOrderId = null;

          if (
            error instanceof HttpErrorResponse &&
            error.status === 401
          ) {
            this.errorMessage =
              'Tu sesión expiró. Inicia sesión nuevamente.';
            this.cdr.detectChanges();
            return;
          }

          if (
            error instanceof HttpErrorResponse &&
            error.status === 403
          ) {
            this.errorMessage =
              'No tienes permisos para validar pedidos.';
            this.cdr.detectChanges();
            return;
          }

          if (
            error instanceof HttpErrorResponse &&
            error.status === 409
          ) {
            this.errorMessage =
              'El pedido no puede avanzar desde su estado actual.';
            this.cdr.detectChanges();
            return;
          }

          this.errorMessage =
            'No fue posible validar el pedido.';
          this.cdr.detectChanges();
        },
      });
  }



  advanceToInventory(order: DeliveryOrder): void {
    if (order.status !== 14) {
      return;
    }

    this.changeOrderStatus(
      order,
      15,
      'Pedido enviado a validación de inventario.',
    );
  }

  validateInventory(order: DeliveryOrder): void {
    if (order.status !== 15) {
      return;
    }

    this.changeOrderStatus(
      order,
      16,
      'Inventario validado correctamente.',
    );
  }

  private changeOrderStatus(order: DeliveryOrder, newStatus: number, reason: string,): void {
    this.updatingOrderId = order.id;
    this.errorMessage = '';
    this.successMessage = '';

    this.ordersService.changeStatus(order.id, {
        newStatus,
        reason,
        ticketReference: null,
        isRollback: false,
      }).subscribe({
        next: (updatedOrder) => {
          this.updatingOrderId = null;

          this.orders = this.orders.map((item) =>
            item.id === updatedOrder.id
              ? updatedOrder
              : item,
          );

          this.applyFilters();

          this.successMessage =
            `El pedido #${updatedOrder.id} cambió a ` +
            `${this.getStatusLabel(updatedOrder.status)}.`;
          this.cdr.detectChanges(); 
        },
        error: (error: unknown) => {
          this.updatingOrderId = null;

          if (
            error instanceof HttpErrorResponse &&
            error.status === 401
          ) {
            this.errorMessage =
              'Tu sesión expiró. Inicia sesión nuevamente.';
            this.cdr.detectChanges();
            return;
          }

          if (
            error instanceof HttpErrorResponse &&
            error.status === 403
          ) {
            this.errorMessage =
              'No tienes permisos para realizar esta acción.';
            this.cdr.detectChanges();
            return;
          }

          if (
            error instanceof HttpErrorResponse &&
            error.status === 409
          ) {
            this.errorMessage =
              'El pedido no puede avanzar desde su estado actual.';
            this.cdr.detectChanges();
            return;
          }

          this.errorMessage =
            'No fue posible cambiar el estado del pedido.';
          this.cdr.detectChanges();
        },
      });
  }


  confirmClientDelivery(order: DeliveryOrder): void {
    if (order.status !== 8) {
      return;
    }

    this.changeOrderStatus(
      order,
      23,
      'Entrega confirmada provisionalmente por el conductor.',
    );
  }

  markDelivered(order: DeliveryOrder): void {
    if (order.status !== 23) {
      return;
    }

    this.changeOrderStatus(
      order,
      9,
      'Pedido entregado al cliente.',
    );
  }

  finishOrder(order: DeliveryOrder): void {
    if (order.status !== 9) {
      return;
    }

    this.changeOrderStatus(
      order,
      24,
      'Ciclo del pedido finalizado.',
    );
  }


  openEvidence(order: DeliveryOrder): void {
    this.evidenceOrder = order;
    this.evidenceFile = null;
    this.evidenceComments = '';
    this.evidenceItems = [];
    this.errorMessage = '';

    this.loadEvidence(order.id);
  }

  closeEvidence(): void {
    if (this.isUploadingEvidence) {
      return;
    }

    this.evidenceOrder = null;
    this.evidenceFile = null;
    this.evidenceComments = '';
    this.evidenceItems = [];
  }

  onEvidenceFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    this.evidenceFile = input.files?.[0] ?? null;
  }

  loadEvidence(orderId: number): void {
    this.isLoadingEvidence = true;

    this.evidenceService.getAll(orderId).subscribe({
      next: (items) => {
        this.evidenceItems = items;
        this.isLoadingEvidence = false;
        this.cdr.detectChanges();
      },
      error: () => {
        this.isLoadingEvidence = false;
        this.errorMessage =
          'No fue posible cargar las evidencias.';
        this.cdr.detectChanges();
      },
    });
  }

  uploadEvidence(): void {
    if (!this.evidenceOrder || !this.evidenceFile) {
      this.errorMessage =
        'Selecciona una fotografía antes de continuar.';
      return;
    }

    const allowedTypes = [
      'image/jpeg',
      'image/png',
      'image/webp',
    ];

    if (!allowedTypes.includes(
      this.evidenceFile.type,
    )) {
      this.errorMessage =
        'Solo se permiten imágenes JPG, PNG o WEBP.';
      return;
    }

    if (this.evidenceFile.size > 10 * 1024 * 1024) {
      this.errorMessage =
        'La imagen no puede superar 10 MB.';
      return;
    }

    this.isUploadingEvidence = true;
    this.errorMessage = '';

    this.evidenceService.upload(
      this.evidenceOrder.id,
      this.evidenceFile,
      this.evidenceComments.trim(),
    ).subscribe({
      next: (evidence) => {
        this.evidenceItems = [
          evidence,
          ...this.evidenceItems,
        ];

        this.evidenceFile = null;
        this.evidenceComments = '';
        this.isUploadingEvidence = false;
        this.successMessage =
          'Evidencia cargada correctamente.';

        this.cdr.detectChanges();
      },
      error: (error: HttpErrorResponse) => {
        this.isUploadingEvidence = false;

        if (error.status === 401) {
          this.errorMessage =
            'Tu sesión expiró.';
        } else if (error.status === 403) {
          this.errorMessage =
            'No tienes permisos para cargar evidencias.';
        } else {
          this.errorMessage =
            error.error?.message ??
            'No fue posible cargar la evidencia.';
        }

        this.cdr.detectChanges();
      },
    });
  }

  downloadEvidence(item: DeliveryEvidence): void {
    this.evidenceService.download(
      item.orderId,
      item.id,
    ).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const anchor = document.createElement('a');

        anchor.href = url;
        anchor.download = item.originalFileName;
        anchor.click();

        URL.revokeObjectURL(url);
      },
      error: () => {
        this.errorMessage =
          'No fue posible descargar la evidencia.';
        this.cdr.detectChanges();
      },
    });
  }

  reschedulePartial(order: DeliveryOrder): void {
    if (order.status !== 22) {
      return;
    }

    this.changeOrderStatus(
      order,
      10,
      'Entrega parcial registrada. Pedido reagendado ' + 'para entregar el saldo pendiente.',
    );
  }

  sendRescheduledToRouting(
    order: DeliveryOrder,
  ): void {
    if (order.status !== 10) {
      return;
    }

    this.changeOrderStatus(
      order,
      19,
      'Pedido con saldo pendiente enviado nuevamente a ruteo.',
    );
  }

  markLoaded(order: DeliveryOrder): void {
    if (order.status !== 6) {
      return;
    }

    this.changeOrderStatus(
      order,
      7,
      'Pedido cargado en el vehículo asignado.',
    );
  }

  validateBySupervisor(order: DeliveryOrder): void {
    if (order.status !== 7) {
      return;
    }

    this.changeOrderStatus(
      order,
      21,
      'Carga validada por el supervisor.',
    );
  }

  startRoute(order: DeliveryOrder): void {
    if (order.status !== 21) {
      return;
    }

    this.changeOrderStatus(
      order,
      8,
      'Pedido autorizado para iniciar ruta.',
    );
  }



  markReadyToLoad(order: DeliveryOrder): void {
    if (order.status !== 20) {
      return;
    }

    this.changeOrderStatus(
      order,
      6,
      'Pedido marcado como listo para cargar.',
    );
  }



  advanceToLogistics(order: DeliveryOrder): void {
    if (order.status !== 16) {
      return;
    }

    this.changeOrderStatus(
      order,
      17,
      'Pedido enviado a validación de logística.',
    );
  }

  validateLogistics(order: DeliveryOrder): void {
    if (order.status !== 17) {
      return;
    }

    this.changeOrderStatus(
      order,
      18,
      'Validación de logística completada.',
    );
  }

  advanceToRouting(order: DeliveryOrder): void {
    if (order.status !== 18) {
      return;
    }

    this.changeOrderStatus(
      order,
      19,
      'Pedido enviado a validación para ruteo.',
    );
  }

  validateToRoute(): void {

  }


  openPartialDelivery(order: DeliveryOrder): void {
    if (order.status !== 8) {
      return;
    }

    this.partialOrder = order;
    this.partialDeliveredWeight = null;
    this.partialReason = '';
    this.partialComments = '';
    this.partialRescheduledDate = '';
    this.errorMessage = '';
    this.successMessage = '';
  }

  cancelPartialDelivery(): void {
    if (this.isSavingPartial) {
      return;
    }

    this.partialOrder = null;
    this.partialDeliveredWeight = null;
    this.partialReason = '';
    this.partialComments = '';
    this.partialRescheduledDate = '';
  }

  savePartialDelivery(): void {
    if (!this.partialOrder) {
      return;
    }

    this.errorMessage = '';
    this.successMessage = '';

    const deliveredWeight =
      this.partialDeliveredWeight ?? 0;

    if (
      deliveredWeight <= 0 ||
      deliveredWeight >= this.partialOrder.weight
    ) {
      this.errorMessage =
        'El peso entregado debe ser mayor que cero y menor ' +
        'que el peso total del pedido.';
      return;
    }

    if (
      this.partialReason.trim().length < 3 ||
      !this.partialRescheduledDate
    ) {
      this.errorMessage =
        'Indica el motivo y la nueva fecha de entrega.';
      return;
    }

    const request: CreatePartialDeliveryRequest = {
      deliveredWeight,
      reason: this.partialReason.trim(),
      comments: this.partialComments.trim() || null,
      rescheduledDate:
        `${this.partialRescheduledDate}T00:00:00`,
    };

    this.isSavingPartial = true;

    this.partialDeliveriesService
      .create(this.partialOrder.id, request)
      .subscribe({
        next: (partialDelivery) => {
          const updatedOrder: DeliveryOrder = {
            ...this.partialOrder!,
            status: 22,
            scheduledDate:
              partialDelivery.rescheduledDate,
          };

          this.orders = this.orders.map((order) =>
            order.id === updatedOrder.id
              ? updatedOrder
              : order,
          );

          this.applyFilters();

          this.isSavingPartial = false;
          this.successMessage =
            `Entrega parcial del pedido #${updatedOrder.id} ` +
            'registrada correctamente.';

          this.cancelPartialDelivery();
          this.cdr.detectChanges();
        },
        error: (error: unknown) => {
          this.isSavingPartial = false;

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
              'No tienes permisos para registrar entregas parciales.';
          } else if (
            error instanceof HttpErrorResponse &&
            (
              error.status === 400 ||
              error.status === 409
            )
          ) {
            this.errorMessage =
              error.error?.message ??
              'La entrega parcial no es válida.';
          } else {
            this.errorMessage =
              'No fue posible registrar la entrega parcial.';
          }

          this.cdr.detectChanges();
        },
      });
  }


  resetCreateForm(): void {
    this.customer = '';
    this.weight = null;
    this.deliveryAddress = '';
    this.sector = '';
    this.scheduledDate = '';
  }

  applyFilters(): void {
    const normalizedSearch =
      this.searchTerm.trim().toLowerCase();

    this.filteredOrders = this.orders.filter((order) => {
      const matchesSearch =
        !normalizedSearch ||
        order.id.toString().includes(normalizedSearch) ||
        order.customer.toLowerCase().includes(normalizedSearch) ||
        order.sector.toLowerCase().includes(normalizedSearch) ||
        order.deliveryAddress.toLowerCase().includes(normalizedSearch);

      const matchesStatus =
        !this.selectedStatus ||
        order.status.toString() === this.selectedStatus;

      return matchesSearch && matchesStatus;
    });
  }

  getStatusLabel(status: number): string {
    return this.statusLabels[status] ??
      `Estado ${status}`;
  }

  trackByOrderId(
    _index: number,
    order: DeliveryOrder,
  ): number {
    return order.id;
  }
}
