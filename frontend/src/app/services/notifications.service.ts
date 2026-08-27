import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api.config';
import { Notification } from '../models/notification.model';

@Injectable({
  providedIn: 'root'
})
export class NotificationsService {
  constructor(private readonly http: HttpClient) {
  }

  getNotifications(): Observable<Notification[]> {
    return this.http.get<Notification[]>(`${API_BASE_URL}/notifications`);
  }

  markAsRead(id: number): Observable<Notification> {
    return this.http.patch<Notification>(`${API_BASE_URL}/notifications/${id}/read`, {});
  }
}
