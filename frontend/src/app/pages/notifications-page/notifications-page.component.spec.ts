import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { Notification } from '../../models/notification.model';
import { User, UserRole } from '../../models/user.model';
import { AuthService } from '../../services/auth.service';
import { NotificationsService } from '../../services/notifications.service';
import { NotificationsPageComponent } from './notifications-page.component';

describe('NotificationsPageComponent permessi', () => {
  let service: jasmine.SpyObj<NotificationsService>;
  let auth: jasmine.SpyObj<AuthService>;
  let notification: Notification;
  const user: User = { id: 7, email: 'user@example.local', role: 'USER', isActive: true };

  beforeEach(() => {
    notification = {
      id: 17, userId: 7, issueId: 5, issueTitle: 'Errore login',
      message: 'La issue e stata risolta.', isRead: false,
      createdAt: '2026-10-16T08:00:00Z'
    };
    service = jasmine.createSpyObj('NotificationsService', ['getNotifications', 'markAsRead']);
    service.getNotifications.and.returnValue(of([
      notification, { ...notification, id: 18, isRead: true }
    ]));
    auth = jasmine.createSpyObj('AuthService', ['getCurrentUser']);
    auth.getCurrentUser.and.returnValue(user);
    TestBed.configureTestingModule({ providers: [
      provideRouter([]),
      { provide: NotificationsService, useValue: service },
      { provide: AuthService, useValue: auth }
    ] });
  });

  it('READONLY consulta notifiche e link senza pulsante di modifica', () => {
    auth.getCurrentUser.and.returnValue({ ...user, role: 'READONLY' });
    const fixture = TestBed.createComponent(NotificationsPageComponent);
    fixture.detectChanges();
    const view = fixture.nativeElement as HTMLElement;
    expect(service.getNotifications).toHaveBeenCalledTimes(1);
    expect(view.querySelectorAll('article.notification').length).toBe(2);
    expect(view.querySelectorAll('button').length).toBe(0);
    expect(view.querySelector('a')?.getAttribute('href')).toBe('/issues/5');
    expect(view.textContent).toContain('Non letta');
  });

  it('READONLY non invia richieste nemmeno chiamando direttamente il metodo', () => {
    auth.getCurrentUser.and.returnValue({ ...user, role: 'READONLY' });
    const page = TestBed.createComponent(NotificationsPageComponent).componentInstance;
    page.markAsRead(17);
    expect(service.markAsRead).not.toHaveBeenCalled();
    expect(page.notifications[0].isRead).toBeFalse();
    expect(page.actionErrorMessage).toBe('Non hai i permessi per questa operazione.');
  });

  for (const role of ['ADMIN', 'USER'] as UserRole[]) {
    it(`${role} mantiene la lettura delle proprie notifiche`, () => {
      auth.getCurrentUser.and.returnValue({ ...user, role });
      service.markAsRead.and.returnValue(of({ ...notification, isRead: true }));
      const fixture = TestBed.createComponent(NotificationsPageComponent);
      fixture.detectChanges();
      const view = fixture.nativeElement as HTMLElement;
      expect(view.querySelectorAll('button').length).toBe(1);
      view.querySelector('button')!.click();
      fixture.detectChanges();
      expect(service.markAsRead).toHaveBeenCalledOnceWith(17);
      expect(fixture.componentInstance.notifications[0].isRead).toBeTrue();
      expect(fixture.componentInstance.notifications[1].isRead).toBeTrue();
      expect(view.querySelectorAll('button').length).toBe(0);
      expect(fixture.componentInstance.readingNotificationId).toBeNull();
    });
  }

  it('mostra un errore chiaro per 403 e non modifica la notifica', () => {
    service.markAsRead.and.returnValue(throwError(() => new HttpErrorResponse({ status: 403 })));
    const page = TestBed.createComponent(NotificationsPageComponent).componentInstance;
    page.markAsRead(17);
    expect(page.actionErrorMessage).toBe('Non hai i permessi per questa operazione.');
    expect(page.notifications[0].isRead).toBeFalse();
    expect(page.readingNotificationId).toBeNull();
  });

  it('senza identita non invia richieste di modifica', () => {
    auth.getCurrentUser.and.returnValue(null);
    const page = TestBed.createComponent(NotificationsPageComponent).componentInstance;
    page.markAsRead(17);
    expect(page.canMarkAsRead).toBeFalse();
    expect(service.markAsRead).not.toHaveBeenCalled();
  });
});
