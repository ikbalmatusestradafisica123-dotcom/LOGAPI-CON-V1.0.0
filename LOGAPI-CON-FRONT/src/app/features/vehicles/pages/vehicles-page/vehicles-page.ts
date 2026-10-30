import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core'; // 1. Importa ChangeDetectorRef
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { CreateVehicleRequest, Vehicle } from '../../../../core/models/vehicles.model';
import { VehiclesService } from '../../../../core/services/vehicles';

@Component({
  selector: 'app-vehicles-page',
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
  ],
  templateUrl: './vehicles-page.html',
  styleUrl: './vehicles-page.scss',
})
export class VehiclesPage implements OnInit {
  private readonly vehiclesService = inject(VehiclesService);
  private readonly cdr = inject(ChangeDetectorRef);

  vehicles: Vehicle[] = [];

  name = '';
  capacity: number | null = null;
  brand = '';
  model = '';
  licensePlate = '';
  isAvailable = true;

  isLoading = false;
  isCreating = false;
  errorMessage = '';
  successMessage = '';

  ngOnInit(): void {
    this.loadVehicles();
  }

  loadVehicles(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.vehiclesService.getAll().subscribe({
      next: (vehicles) => {
        this.vehicles = vehicles;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (error: unknown) => {
        this.isLoading = false;
        this.errorMessage = this.getErrorMessage(
          error,
          'No fue posible cargar los vehículos.'
        );
        this.cdr.detectChanges();
      },
    });
  }

  createVehicle(): void {
    this.errorMessage = '';
    this.successMessage = '';

    const request: CreateVehicleRequest = {
      name: this.name.trim(),
      capacity: this.capacity ?? 0,
      brand: this.brand.trim(),
      model: this.model.trim(),
      licensePlate: this.licensePlate.trim(),
      isAvailable: this.isAvailable,
    };

    if (
      request.name.length < 3 ||
      request.capacity <= 0 ||
      request.brand.length === 0 ||
      request.model.length === 0 ||
      request.licensePlate.length === 0
    ) {
      this.errorMessage = 'Completa correctamente todos los campos.';
      return;
    }

    this.isCreating = true;

    this.vehiclesService.create(request).subscribe({
      next: (vehicle) => {
        this.vehicles = [...this.vehicles, vehicle];
        this.resetForm();
        this.isCreating = false;
        this.successMessage = `Vehículo ${vehicle.name} creado correctamente.`;
        this.cdr.detectChanges(); 
      },
      error: (error: unknown) => {
        this.isCreating = false;
        this.errorMessage = this.getErrorMessage(
          error,
          'No fue posible crear el vehículo.'
        );
        this.cdr.detectChanges(); 
      },
    });
  }

  private resetForm(): void {
    this.name = '';
    this.capacity = null;
    this.brand = '';
    this.model = '';
    this.licensePlate = '';
    this.isAvailable = true;
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse && error.status === 401) {
      return 'Tu sesión expiró.';
    }
    if (error instanceof HttpErrorResponse && error.status === 403) {
      return 'No tienes permisos para administrar vehículos.';
    }
    if (error instanceof HttpErrorResponse && error.status === 400) {
      return error.error?.message ?? 'Los datos del vehículo no son válidos.';
    }
    return fallback;
  }
}
