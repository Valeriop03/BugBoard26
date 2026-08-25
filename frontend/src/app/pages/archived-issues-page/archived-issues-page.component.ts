import { Component, inject } from '@angular/core';
import { RouterLink } from '@angular/router';
import { Issue } from '../../models/issue.model';
import { IssuesService } from '../../services/issues.service';

@Component({
  selector: 'app-archived-issues-page',
  imports: [RouterLink],
  templateUrl: './archived-issues-page.component.html',
  styleUrl: './archived-issues-page.component.css'
})
export class ArchivedIssuesPageComponent {
  private readonly issuesService = inject(IssuesService);
  archivedIssues: Issue[] = [];
  isLoading = true;
  errorMessage = '';

  constructor() {
    this.issuesService.getArchivedIssues().subscribe({
      next: issues => {
        this.archivedIssues = issues;
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Non e stato possibile caricare le issue archiviate.';
        this.isLoading = false;
      }
    });
  }
}
