import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { HttpErrorResponse } from '@angular/common/http';

import {
  PlanRouteResponse,
  RoutePlan,
} from '../../../../core/models/routes.model';
import { RoutesService } from '../../../../core/services/routes';

@Component({
  selector: 'app-routes-page',
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
  ],
  templateUrl: './routes-page.html',
  styleUrl: './routes-page.scss',
})
export class RoutesPage {
  private readonly routesService = inject(RoutesService);

  routeDate = new Date()
    .toISOString()
    .slice(0, 10);

  maxTonnagePerDay = 20;

  isLoading = false;
  errorMessage = '';
  successMessage = '';

  routeResponse: PlanRouteResponse | null = null;

  planRoute(): void {
    this.errorMessage = '';
    this.successMessage = '';
    this.routeResponse = null;

    if (
      !this.routeDate ||
      this.maxTonnagePerDay <= 0
    ) {
      this.errorMessage =
        'Indica una fecha y un límite válido de pedidos.';
      return;
    }

    this.isLoading = true;

    this.routesService
      .plan({
        date: `${this.routeDate}T00:00:00`,
        maxTonnagePerDay: this.maxTonnagePerDay,
      })
      .subscribe({
        next: (response) => {
          this.isLoading = false;
          this.routeResponse = response;

          this.successMessage =
            'La ruta se planificó correctamente.';
        },
        error: (error: unknown) => {
          this.isLoading = false;

          if (
            error instanceof HttpErrorResponse &&
            error.status === 401
          ) {
            this.errorMessage =
              'Tu sesión expiró. Inicia sesión nuevamente.';
            return;
          }

          if (
            error instanceof HttpErrorResponse &&
            error.status === 403
          ) {
            this.errorMessage =
              'No tienes permisos para planificar rutas.';
            return;
          }

          if (
            error instanceof HttpErrorResponse &&
            error.status === 400
          ) {
            this.errorMessage =
              'Los datos de planificación no son válidos.';
            return;
          }

          this.errorMessage =
            'No fue posible planificar la ruta.';
        },
      });
  }

  totalWeight(route: RoutePlan): string {
    return route.totalWeight.toFixed(2);
  }

  distanceInKilometers(route: RoutePlan): string {
    return (route.distance / 1000).toFixed(2);
  }

  durationInMinutes(route: RoutePlan): string {
    return (route.duration / 60).toFixed(0);
  }
}
