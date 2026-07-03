import { HttpClient } from '@angular/common/http';
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

  getIssues(): Observable<Issue[]> {
    return this.http.get<ApiIssue[]>(`${API_BASE_URL}/issues`)
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
