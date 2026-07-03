import { Component } from '@angular/core';
import { inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Issue } from '../../models/issue.model';
import { IssuesService } from '../../services/issues.service';

@Component({
  selector: 'app-issues-page',
  imports: [RouterLink],
  templateUrl: './issues-page.component.html',
  styleUrl: './issues-page.component.css'
})
export class IssuesPageComponent {
  private readonly issuesService = inject(IssuesService);
  issues: Issue[] = [];
  isLoading = true;
  errorMessage = '';

  constructor() {
    this.loadIssues();
  }

  private loadIssues(): void {
    this.issuesService.getIssues().subscribe({
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
}
