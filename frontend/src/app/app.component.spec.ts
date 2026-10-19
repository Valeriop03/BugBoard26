import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { AppComponent } from './app.component';
import { User } from './models/user.model';
import { AuthService } from './services/auth.service';

describe('AppComponent navigation', () => {
  for (const role of ['ADMIN', 'USER', 'READONLY'] as const) {
    it(`shows user management only for ADMIN (${role})`, async () => {
      const user: User = { id: 1, email: 'navigation@example.test', role, isActive: true };
      await TestBed.configureTestingModule({
        imports: [AppComponent],
        providers: [provideRouter([]), { provide: AuthService, useValue: { getCurrentUser: () => user } }]
      }).compileComponents();
      const fixture = TestBed.createComponent(AppComponent);

      fixture.detectChanges();

      const usersLink = fixture.nativeElement.querySelector('a[href="/users"]');
      expect(!!usersLink).toBe(role === 'ADMIN');
      expect(fixture.nativeElement.querySelector('.role').textContent).toContain(role);
    });
  }
});
