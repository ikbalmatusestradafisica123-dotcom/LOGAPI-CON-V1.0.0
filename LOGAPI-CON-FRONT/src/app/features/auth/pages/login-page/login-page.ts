import { Component, inject, ChangeDetectorRef } from '@angular/core'; // <--- 1. Importa ChangeDetectorRef
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { OrdersService } from '../../../../core/services/orders';
import { DeliveryOrder, orderStatusLabels } from '../../../../core/models/orders.model';
import { Auth } from '../../../../core/services/auth';
import { HttpErrorResponse } from '@angular/common/http';
import { finalize } from 'rxjs';
import { CommonModule, DecimalPipe } from '@angular/common';

@Component({
  selector: 'app-login-page',
  imports: [CommonModule, FormsModule, DecimalPipe],
  templateUrl: './login-page.html',
  styleUrl: './login-page.scss',
})
export class LoginPage {
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);
  private readonly ordersService = inject(OrdersService);
  private readonly cdr = inject(ChangeDetectorRef); // <--- 2. Inyecta ChangeDetectorRef aquí

  activeLogin: 'user' | 'client' | null = null;

  // Variables de Login de Personal
  username = '';
  password = '';

  // Variables de Consulta de Clientes
  clientOrderNumberInput = '';
  showClientCard = false;
  consultedOrder: DeliveryOrder | null = null;

  isLoading = false;
  errorMessage = '';

  readonly statusLabels = orderStatusLabels;

  openLogin(): void {
    this.activeLogin = 'user';
    this.errorMessage = '';
  }

  closeLogin(): void {
    this.activeLogin = null;
    this.errorMessage = '';
    this.password = '';
  }

  openClientLogin(): void {
    this.activeLogin = 'client';
    this.errorMessage = '';
  }

  closeClientLogin(): void {
    this.activeLogin = null;
    this.errorMessage = '';
    this.clientOrderNumberInput = '';
    this.showClientCard = false;
    this.consultedOrder = null;
  }

  login(): void {
    this.errorMessage = '';

    if (!this.username.trim() || !this.password) {
      this.errorMessage = 'Ingresa tu usuario y contraseña.';
      return;
    }

    this.isLoading = true;

    this.auth
      .login({
        username: this.username.trim(),
        password: this.password,
      })
      .pipe(finalize(() => { this.isLoading = false; }))
      .subscribe({
        next: () => {
          this.password = '';
          void this.router.navigateByUrl('/dashboard');
        },
        error: (error: unknown) => {
          this.password = '';

          if (error instanceof HttpErrorResponse && error.status === 401) {
            this.errorMessage = 'Usuario o contraseña incorrectos.';
            return;
          }

          if (error instanceof HttpErrorResponse && error.status === 0) {
            this.errorMessage = 'No fue posible conectarse con la API.';
            return;
          }

          this.errorMessage = 'Ocurrió un error al iniciar sesión.';
        },
      });
  }

  // Método para consultar pedido del cliente (Solo por número de pedido)
  clientOrder(): void {
    this.errorMessage = '';
    this.consultedOrder = null;
    this.showClientCard = false;

    if (!this.clientOrderNumberInput.trim()) {
      this.errorMessage = 'Debe ingresar un número de pedido.';
      return;
    }

    this.isLoading = true;
    const idOrderNumber = Number(this.clientOrderNumberInput.trim());

    this.ordersService.getById(idOrderNumber)
      .pipe(finalize(() => {
        this.isLoading = false;
        this.cdr.detectChanges(); // Asegura que apague el loading visualmente
      }))
      .subscribe({
        next: (foundedOrder) => {
          this.consultedOrder = foundedOrder;
          this.showClientCard = true;

          // <--- 3. FUERZA A ANGULAR A RENDERIZAR EL MODAL INMEDIATAMENTE AL RECIBIR DATOS
          this.cdr.detectChanges();

          console.log('Pedido del cliente encontrado:', foundedOrder);
        },
        error: (error: unknown) => {
          if (error instanceof HttpErrorResponse && error.status === 404) {
            this.errorMessage = 'No se encontró ningún pedido con ese número.';
          } else if (error instanceof HttpErrorResponse && error.status === 401) {
            this.errorMessage = 'Acceso no autorizado al recurso.';
          } else if (error instanceof HttpErrorResponse && error.status === 0) {
            this.errorMessage = 'No fue posible conectarse con la API.';
          } else {
            this.errorMessage = 'Ocurrió un error al consultar el pedido.';
          }
          this.cdr.detectChanges(); // Fuerza refresco del mensaje de error
        }
      });
  }

  // Métodos de apoyo visual para la tarjeta del cliente
  getStatusLabel(status: number): string {
    return this.statusLabels[status] ?? `Estado ${status}`;
  }

  getProgressPercentage(status: number): number {
    switch (status) {
      case 7: return 10;
      case 8: return 50;
      case 9:
      case 24: return 100;
      default: return 0;
    }
  }

  // Método para cerrar y limpiar la tarjeta modal del cliente
  resetClientSearch(): void {
    this.consultedOrder = null;
    this.showClientCard = false;
    this.clientOrderNumberInput = '';
    this.cdr.detectChanges();
  }
}
