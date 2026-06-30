import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { User } from '../models/user.model';

@Injectable({
  providedIn: 'root'
})
export class UsersService {
  getUsers(): Observable<User[]> {
    return of([
      { id: 1, email: 'admin@bugboard26.local', role: 'ADMIN', isActive: true },
      { id: 2, email: 'dev@bugboard26.local', role: 'USER', isActive: true },
      { id: 3, email: 'stakeholder@bugboard26.local', role: 'READONLY', isActive: true }
    ]);
  }
}
