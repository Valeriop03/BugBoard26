import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, map } from 'rxjs';
import { API_BASE_URL } from '../api.config';
import { Issue, IssuePriority, IssueStatus, IssueType } from '../models/issue.model';
import { SuggestedAssignee } from '../models/suggested-assignee.model';

type ApiIssueType = 'Question' | 'Bug' | 'Documentation' | 'Feature';
type ApiIssuePriority = 'Low' | 'Medium' | 'High' | 'Critical';
type ApiIssueStatus = 'Todo' | 'InProgress' | 'Resolved' | 'Closed' | 'Duplicate';

interface ApiIssue {
  id: number;
  title: string;
  description: string;
  createdById: number;
  type: ApiIssueType;
  priority: ApiIssuePriority | null;
  status: ApiIssueStatus;
  assignedToId: number | null;
  createdByEmail: string;
  assignedToEmail: string | null;
  duplicateOfIssueId: number | null;
  isArchived: boolean;
  createdAt: string;
  updatedAt: string;
  resolvedAt: string | null;
}

interface ApiSuggestedAssignee {
  userId: number;
  email: string;
  openAssignedIssues: number;
}

export interface CreateIssueRequest {
  title: string;
  description: string;
  type: IssueType;
  priority?: IssuePriority | null;
  assignedToId?: number | null;
}

export interface IssueListFilters {
  keyword?: string;
  type?: IssueType | '';
  status?: IssueStatus | '';
  priority?: IssuePriority | '';
  sortBy?: 'Date' | 'Priority' | 'Status' | 'Title';
  sortDirection?: 'Asc' | 'Desc';
}

const issueTypeToApiMap: Record<IssueType, ApiIssueType> = {
  QUESTION: 'Question',
  BUG: 'Bug',
  DOCUMENTATION: 'Documentation',
  FEATURE: 'Feature'
};

const issuePriorityToApiMap: Record<IssuePriority, ApiIssuePriority> = {
  LOW: 'Low',
  MEDIUM: 'Medium',
  HIGH: 'High',
  CRITICAL: 'Critical'
};

const issueStatusFromApiMap: Record<ApiIssueStatus, IssueStatus> = {
  Todo: 'TODO',
  InProgress: 'IN_PROGRESS',
  Resolved: 'RESOLVED',
  Closed: 'CLOSED',
  Duplicate: 'DUPLICATE'
};

const issueStatusToApiMap: Record<IssueStatus, ApiIssueStatus> = {
  TODO: 'Todo',
  IN_PROGRESS: 'InProgress',
  RESOLVED: 'Resolved',
  CLOSED: 'Closed',
  DUPLICATE: 'Duplicate'
};

const issueTypeFromApiMap: Record<ApiIssueType, IssueType> = {
  Question: 'QUESTION',
  Bug: 'BUG',
  Documentation: 'DOCUMENTATION',
  Feature: 'FEATURE'
};

const issuePriorityFromApiMap: Record<ApiIssuePriority, IssuePriority> = {
  Low: 'LOW',
  Medium: 'MEDIUM',
  High: 'HIGH',
  Critical: 'CRITICAL'
};

@Injectable({
  providedIn: 'root'
})
export class IssuesService {
  constructor(private readonly http: HttpClient) {
  }

  getIssues(filters: IssueListFilters = {}): Observable<Issue[]> {
    return this.http.get<ApiIssue[]>(`${API_BASE_URL}/issues`, {
      params: this.buildIssueParams(filters)
    })
      .pipe(map(issues => issues.map(issue => this.mapIssue(issue))));
  }

  getArchivedIssues(): Observable<Issue[]> {
    return this.http.get<ApiIssue[]>(`${API_BASE_URL}/issues/archived`)
      .pipe(map(issues => issues.map(issue => this.mapIssue(issue))));
  }

  getIssueById(id: number): Observable<Issue> {
    return this.http.get<ApiIssue>(`${API_BASE_URL}/issues/${id}`)
      .pipe(map(issue => this.mapIssue(issue)));
  }

  createIssue(request: CreateIssueRequest): Observable<Issue> {
    return this.http.post<ApiIssue>(`${API_BASE_URL}/issues`, {
      title: request.title,
      description: request.description,
      type: issueTypeToApiMap[request.type],
      priority: request.priority ? issuePriorityToApiMap[request.priority] : null,
      assignedToId: request.assignedToId ?? null
    }).pipe(map(issue => this.mapIssue(issue)));
  }

  suggestAssignee(): Observable<SuggestedAssignee> {
    return this.http.get<ApiSuggestedAssignee>(`${API_BASE_URL}/issues/suggest-assignee`);
  }

  updateStatus(id: number, status: IssueStatus): Observable<Issue> {
    return this.http.patch<ApiIssue>(`${API_BASE_URL}/issues/${id}/status`, {
      status: issueStatusToApiMap[status]
    }).pipe(map(issue => this.mapIssue(issue)));
  }

  archiveIssue(id: number): Observable<Issue> {
    return this.http.patch<ApiIssue>(`${API_BASE_URL}/issues/${id}/archive`, {})
      .pipe(map(issue => this.mapIssue(issue)));
  }

  markAsDuplicate(id: number, originalIssueId: number): Observable<Issue> {
    return this.http.patch<ApiIssue>(`${API_BASE_URL}/issues/${id}/duplicate`, { originalIssueId })
      .pipe(map(issue => this.mapIssue(issue)));
  }

  exportIssues(filters: IssueListFilters = {}): Observable<Blob> {
    return this.http.get(`${API_BASE_URL}/issues/export`, {
      params: this.buildIssueParams(filters),
      responseType: 'blob'
    });
  }

  private buildIssueParams(filters: IssueListFilters): HttpParams {
    let params = new HttpParams();

    if (filters.keyword?.trim()) {
      params = params.set('keyword', filters.keyword.trim());
    }

    if (filters.type) {
      params = params.set('type', issueTypeToApiMap[filters.type]);
    }

    if (filters.status) {
      params = params.set('status', issueStatusToApiMap[filters.status]);
    }

    if (filters.priority) {
      params = params.set('priority', issuePriorityToApiMap[filters.priority]);
    }

    if (filters.sortBy) {
      params = params.set('sortBy', filters.sortBy);
    }

    if (filters.sortDirection) {
      params = params.set('sortDirection', filters.sortDirection);
    }

    return params;
  }

  private mapIssue(issue: ApiIssue): Issue {
    return {
      id: issue.id,
      title: issue.title,
      description: issue.description,
      createdById: issue.createdById,
      type: issueTypeFromApiMap[issue.type],
      priority: issue.priority ? issuePriorityFromApiMap[issue.priority] : null,
      status: issueStatusFromApiMap[issue.status],
      assignedToId: issue.assignedToId,
      createdByEmail: issue.createdByEmail,
      assignedToEmail: issue.assignedToEmail,
      duplicateOfIssueId: issue.duplicateOfIssueId,
      isArchived: issue.isArchived,
      createdAt: issue.createdAt,
      updatedAt: issue.updatedAt,
      resolvedAt: issue.resolvedAt
    };
  }
}
