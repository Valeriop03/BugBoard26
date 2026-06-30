export type IssueType = 'QUESTION' | 'BUG' | 'DOCUMENTATION' | 'FEATURE';
export type IssuePriority = 'LOW' | 'MEDIUM' | 'HIGH' | 'CRITICAL';
export type IssueStatus = 'TODO' | 'IN_PROGRESS' | 'RESOLVED' | 'CLOSED' | 'DUPLICATE';

export interface Issue {
  id: number;
  title: string;
  description: string;
  type: IssueType;
  priority?: IssuePriority;
  status: IssueStatus;
  createdByEmail: string;
  assignedToEmail?: string;
  createdAt: string;
}
