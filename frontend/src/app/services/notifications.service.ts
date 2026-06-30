import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { Notification } from '../models/notification.model';

@Injectable({
  providedIn: 'root'
})
export class NotificationsService {
  getNotifications(): Observable<Notification[]> {
    return of([
      {
        id: 1,
        issueId: 1,
        message: 'La issue "Errore login" e stata risolta.',
        isRead: false,
        createdAt: '2026-06-26'
      }
    ]);
  }
}
