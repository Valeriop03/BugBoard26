import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { API_BASE_URL } from '../api.config';
import { Issue } from '../models/issue.model';

@Injectable({
  providedIn: 'root'
})
export class IssuesService {
  constructor(private readonly http: HttpClient) {
  }

  getIssues(): Observable<Issue[]> {
    return this.http.get<Issue[]>(`${API_BASE_URL}/issues`);
  }

  getArchivedIssues(): Observable<Issue[]> {
    return of([]);
  }

  getIssueById(id: number): Observable<Issue | undefined> {
    return of(undefined);
  }
}
