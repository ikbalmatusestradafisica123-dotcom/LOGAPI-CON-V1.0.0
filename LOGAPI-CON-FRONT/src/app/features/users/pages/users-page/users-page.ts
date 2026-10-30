import { CommonModule } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, ChangeDetectorRef } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import {
  CreateUserRequest,
  UserResponse,
} from '../../../../core/models/users.model';
import { UsersService } from '../../../../core/services/users';

@Component({
  selector: 'app-users-page',
  imports: [
    CommonModule,
    FormsModule,
    RouterLink,
  ],
  templateUrl: './users-page.html',
  styleUrl: './users-page.scss',
})
export class UsersPage implements OnInit {
  private readonly usersService = inject(UsersService);
  private readonly cdr = inject(ChangeDetectorRef);

  users: UserResponse[] = [];

  readonly roles = [
    'Administrator',
    'Owner',
    'Logistics',
    'Supervisor',
    'Driver',
    'Inventory',
    'Sales',
    'Purchasing',
    'Warehouse',
  ];

  username = '';
  password = '';
  role = 'Logistics';

  isLoading = false;
  isCreating = false;
  errorMessage = '';
  successMessage = '';

  ngOnInit(): void {
    this.loadUsers();
  }

  loadUsers(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.usersService.getAll().subscribe({
      next: (users) => {
        this.users = users;
        this.isLoading = false;
        this.cdr.detectChanges();
      },
      error: (error: unknown) => {
        this.isLoading = false;
        this.errorMessage = this.getErrorMessage(
          error,
          'No fue posible cargar los usuarios.',
        );
        this.cdr.detectChanges();
      },
    });
  }

  createUser(): void {
    this.errorMessage = '';
    this.successMessage = '';

    const request: CreateUserRequest = {
      username: this.username.trim(),
      password: this.password,
      role: this.role,
    };

    if (
      request.username.length < 3 ||
      request.password.length < 12 ||
      !request.role
    ) {
      this.errorMessage =
        'Usuario, contraseña y rol son obligatorios. ' +
        'La contraseña debe tener al menos 12 caracteres.';
      return;
    }

    this.isCreating = true;

    this.usersService.create(request).subscribe({
      next: (user) => {
        this.users = [
          ...this.users,
          user,
        ].sort((a, b) =>
          a.username.localeCompare(b.username),
        );

        this.username = '';
        this.password = '';
        this.role = 'Logistics';
        this.isCreating = false;
        this.successMessage =
          `Usuario ${user.username} creado correctamente.`;
        this.cdr.detectChanges();
      },
      error: (error: unknown) => {
        this.isCreating = false;
        this.errorMessage = this.getErrorMessage(
          error,
          'No fue posible crear el usuario.',
        );
        this.cdr.detectChanges();
      },
    });
  }

  changeRole(
    user: UserResponse,
    role: string,
  ): void {
    if (!role || role === user.role) {
      return;
    }

    this.usersService
      .updateRole(user.id, { role })
      .subscribe({
        next: (updatedUser) => {
          this.replaceUser(updatedUser);
          this.successMessage =
            `Rol de ${updatedUser.username} actualizado.`;
          this.cdr.detectChanges();
        },
        error: (error: unknown) => {
          this.errorMessage = this.getErrorMessage(
            error,
            'No fue posible actualizar el rol.',
          );
          this.loadUsers();
          this.cdr.detectChanges();
        },
      });
  }

  toggleStatus(user: UserResponse): void {
    this.usersService
      .updateStatus(user.id, {
        isActive: !user.isActive,
      })
      .subscribe({
        next: (updatedUser) => {
          this.replaceUser(updatedUser);
          this.successMessage =
            `Estado de ${updatedUser.username} actualizado.`;
          this.cdr.detectChanges();
        },
        error: (error: unknown) => {
          this.errorMessage = this.getErrorMessage(
            error,
            'No fue posible actualizar el estado.',
          );
          this.cdr.detectChanges();
        },
      });
  }

  private replaceUser(updatedUser: UserResponse): void {
    this.users = this.users.map((user) =>
      user.id === updatedUser.id
        ? updatedUser
        : user,
    );
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
      return 'No tienes permisos para esta operación.';
    }

    if (
      error instanceof HttpErrorResponse &&
      error.status === 409
    ) {
      return error.error?.message ??
        'La operación entra en conflicto con una regla del sistema.';
    }

    if (
      error instanceof HttpErrorResponse &&
      error.status === 400
    ) {
      return error.error?.message ??
        'Los datos enviados no son válidos.';
    }

    return fallback;
  }
}
