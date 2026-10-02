import { Component } from '@angular/core';
import { inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { Issue, IssuePriority, IssueStatus, IssueType } from '../../models/issue.model';
import { IssueListFilters, IssuesService } from '../../services/issues.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-issues-page',
  imports: [FormsModule, RouterLink],
  templateUrl: './issues-page.component.html',
  styleUrl: './issues-page.component.css'
})
export class IssuesPageComponent {
  private readonly issuesService = inject(IssuesService);
  private readonly authService = inject(AuthService);

  get canCreateIssue(): boolean {
    const role = this.authService.getCurrentUser()?.role;
    return role === 'ADMIN' || role === 'USER';
  }

  readonly typeOptions: IssueType[] = ['QUESTION', 'BUG', 'DOCUMENTATION', 'FEATURE'];
  readonly statusOptions: IssueStatus[] = ['TODO', 'IN_PROGRESS', 'RESOLVED', 'CLOSED', 'DUPLICATE'];
  readonly priorityOptions: IssuePriority[] = ['LOW', 'MEDIUM', 'HIGH', 'CRITICAL'];
  readonly sortOptions = [
    { value: 'Date-Desc', label: 'Data creazione desc' },
    { value: 'Date-Asc', label: 'Data creazione asc' },
    { value: 'Priority-Desc', label: 'Priorita desc' },
    { value: 'Priority-Asc', label: 'Priorita asc' },
    { value: 'Status-Asc', label: 'Stato asc' },
    { value: 'Title-Asc', label: 'Titolo asc' }
  ];

  issues: Issue[] = [];
  isLoading = true;
  isExporting = false;
  errorMessage = '';
  exportMessage = '';

  keyword = '';
  type: IssueType | '' = '';
  status: IssueStatus | '' = '';
  priority: IssuePriority | '' = '';
  sort = 'Date-Desc';
  private searchTimer: ReturnType<typeof setTimeout> | null = null;

  constructor() {
    this.loadIssues();
  }

  loadIssues(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.issuesService.getIssues(this.buildFilters()).subscribe({
      next: issues => {
        this.issues = issues;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Non e stato possibile caricare le issue.';
        this.isLoading = false;
      }
    });
  }

  resetFilters(): void {
    this.keyword = '';
    this.type = '';
    this.status = '';
    this.priority = '';
    this.sort = 'Date-Desc';
    this.loadIssues();
  }

  onSearchChange(): void {
    if (this.searchTimer) {
      clearTimeout(this.searchTimer);
    }

    this.searchTimer = setTimeout(() => this.loadIssues(), 350);
  }

  exportCsv(): void {
    if (this.isExporting) {
      return;
    }

    this.isExporting = true;
    this.exportMessage = '';

    this.issuesService.exportIssues(this.buildFilters())
      .pipe(finalize(() => {
        this.isExporting = false;
      }))
      .subscribe({
        next: file => {
          this.downloadFile(file);
          this.exportMessage = 'Export CSV completato.';
        },
        error: () => {
          this.exportMessage = 'Non e stato possibile esportare le issue.';
        }
      });
  }

  private buildFilters(): IssueListFilters {
    const [sortBy, sortDirection] = this.sort.split('-') as ['Date' | 'Priority' | 'Status' | 'Title', 'Asc' | 'Desc'];

    return {
      keyword: this.keyword,
      type: this.type,
      status: this.status,
      priority: this.priority,
      sortBy,
      sortDirection
    };
  }

  private downloadFile(file: Blob): void {
    const url = window.URL.createObjectURL(file);
    const link = document.createElement('a');

    link.href = url;
    link.download = 'issues.csv';
    link.click();

    window.URL.revokeObjectURL(url);
  }
}
