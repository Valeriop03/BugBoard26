import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { Notification } from '../../models/notification.model';
import { NotificationsService } from '../../services/notifications.service';

@Component({
  selector: 'app-notifications-page',
  imports: [DatePipe, RouterLink],
  templateUrl: './notifications-page.component.html',
  styleUrl: './notifications-page.component.css'
})
export class NotificationsPageComponent {
  private readonly notificationsService = inject(NotificationsService);

  notifications: Notification[] = [];
  isLoading = true;
  errorMessage = '';
  actionErrorMessage = '';
  readingNotificationId: number | null = null;

  constructor() {
    this.loadNotifications();
  }

  markAsRead(notificationId: number): void {
    if (this.readingNotificationId === notificationId) {
      return;
    }

    this.readingNotificationId = notificationId;
    this.actionErrorMessage = '';

    this.notificationsService.markAsRead(notificationId)
      .pipe(finalize(() => {
        this.readingNotificationId = null;
      }))
      .subscribe({
        next: updatedNotification => {
          this.notifications = this.notifications.map(notification =>
            notification.id === updatedNotification.id ? updatedNotification : notification);
        },
        error: error => {
          this.actionErrorMessage = this.getErrorMessage(error, 'Non e stato possibile aggiornare la notifica.');
        }
      });
  }

  private loadNotifications(): void {
    this.notificationsService.getNotifications()
      .pipe(finalize(() => {
        this.isLoading = false;
      }))
      .subscribe({
        next: notifications => {
          this.notifications = notifications;
          this.errorMessage = '';
        },
        error: error => {
          this.errorMessage = this.getErrorMessage(error, 'Non e stato possibile caricare le notifiche.');
        }
      });
  }

  private getErrorMessage(error: unknown, fallbackMessage: string): string {
    if (error instanceof HttpErrorResponse && typeof error.error?.message === 'string') {
      return error.error.message;
    }

    return fallbackMessage;
  }
}
