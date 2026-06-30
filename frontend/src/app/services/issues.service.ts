import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { Issue } from '../models/issue.model';

@Injectable({
  providedIn: 'root'
})
export class IssuesService {
  private readonly sampleIssues: Issue[] = [
    {
      id: 1,
      title: 'Errore login',
      description: 'La login fallisce anche con password corretta.',
      type: 'BUG',
      priority: 'HIGH',
      status: 'TODO',
      createdByEmail: 'mario.rossi@example.com',
      assignedToEmail: 'dev@example.com',
      createdAt: '2026-06-24'
    },
    {
      id: 2,
      title: 'Chiarimento export',
      description: 'Serve capire quali campi inserire nel CSV.',
      type: 'QUESTION',
      priority: 'LOW',
      status: 'IN_PROGRESS',
      createdByEmail: 'anna.verdi@example.com',
      assignedToEmail: 'admin@example.com',
      createdAt: '2026-06-25'
    }
  ];

  getIssues(): Observable<Issue[]> {
    return of(this.sampleIssues);
  }

  getArchivedIssues(): Observable<Issue[]> {
    return of([]);
  }

  getIssueById(id: number): Observable<Issue | undefined> {
    return of(this.sampleIssues.find(issue => issue.id === id));
  }
}
