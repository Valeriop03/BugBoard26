import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { finalize } from 'rxjs';
import { User, UserRole } from '../../models/user.model';
import { CreateUserRequest, UsersService } from '../../services/users.service';

@Component({
  selector: 'app-users-page',
  imports: [FormsModule],
  templateUrl: './users-page.component.html',
  styleUrl: './users-page.component.css'
})
export class UsersPageComponent {
  private readonly usersService = inject(UsersService);

  readonly roleOptions: UserRole[] = ['ADMIN', 'USER', 'READONLY'];

  users: User[] = [];
  email = '';
  password = '';
  role: UserRole = 'USER';
  isLoading = true;
  isSubmitting = false;
  errorMessage = '';
  createErrorMessage = '';
  createSuccessMessage = '';

  constructor() {
    this.loadUsers();
  }

  createUser(): void {
    if (this.isSubmitting) {
      return;
    }

    this.isSubmitting = true;
    this.createErrorMessage = '';
    this.createSuccessMessage = '';

    const request: CreateUserRequest = {
      email: this.email,
      password: this.password,
      role: this.role
    };

    this.usersService.createUser(request)
      .pipe(finalize(() => {
        this.isSubmitting = false;
      }))
      .subscribe({
        next: createdUser => {
          this.users = [...this.users, createdUser]
            .sort((left, right) => left.email.localeCompare(right.email));
          this.email = '';
          this.password = '';
          this.role = 'USER';
          this.createSuccessMessage = 'Utente creato correttamente.';
        },
        error: error => {
          this.createErrorMessage = this.getRequestErrorMessage(error, 'Non e stato possibile creare l\'utente.');
        }
      });
  }

  private loadUsers(): void {
    this.usersService.getUsers()
      .pipe(finalize(() => {
        this.isLoading = false;
      }))
      .subscribe({
        next: users => {
          this.users = users;
          this.errorMessage = '';
        },
        error: error => {
          this.errorMessage = this.getRequestErrorMessage(error, 'Non e stato possibile caricare gli utenti.');
        }
      });
  }

  private getRequestErrorMessage(error: unknown, fallbackMessage: string): string {
    if (error instanceof HttpErrorResponse) {
      if (typeof error.error?.message === 'string') {
        return error.error.message;
      }

      const validationErrors = error.error?.errors;

      if (validationErrors && typeof validationErrors === 'object') {
        for (const value of Object.values(validationErrors)) {
          if (Array.isArray(value) && value.length > 0 && typeof value[0] === 'string') {
            return value[0];
          }
        }
      }
    }

    return fallbackMessage;
  }
}
