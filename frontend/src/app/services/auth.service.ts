import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { User } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  login(email: string, password: string): Observable<void> {
    return of(void 0);
  }

  logout(): void {
    localStorage.removeItem('bugboard26-token');
  }

  getCurrentUser(): User | null {
    return null;
  }
}
