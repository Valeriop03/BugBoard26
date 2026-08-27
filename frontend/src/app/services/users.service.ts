import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../api.config';
import { User } from '../models/user.model';

export interface CreateUserRequest {
  email: string;
  password: string;
  role: User['role'];
}

@Injectable({
  providedIn: 'root'
})
export class UsersService {
  constructor(private readonly http: HttpClient) {
  }

  getUsers(): Observable<User[]> {
    return this.http.get<User[]>(`${API_BASE_URL}/users`);
  }

  createUser(request: CreateUserRequest): Observable<User> {
    return this.http.post<User>(`${API_BASE_URL}/users`, request);
  }
}
