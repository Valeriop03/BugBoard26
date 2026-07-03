export type IssueType = 'QUESTION' | 'BUG' | 'DOCUMENTATION' | 'FEATURE';
export type IssuePriority = 'LOW' | 'MEDIUM' | 'HIGH' | 'CRITICAL';
export type IssueStatus = 'TODO' | 'IN_PROGRESS' | 'RESOLVED' | 'CLOSED' | 'DUPLICATE';

export interface Issue {
  id: number;
  title: string;
  description: string;
  createdById: number;
  type: IssueType;
  priority: IssuePriority | null;
  status: IssueStatus;
  assignedToId: number | null;
  createdByEmail: string;
  assignedToEmail: string | null;
  duplicateOfIssueId: number | null;
  isArchived: boolean;
  createdAt: string;
  updatedAt: string;
  resolvedAt: string | null;
}
