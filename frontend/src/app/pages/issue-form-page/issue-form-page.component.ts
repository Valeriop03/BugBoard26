import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { IssuePriority, IssueType } from '../../models/issue.model';
import { SuggestedAssignee } from '../../models/suggested-assignee.model';
import { CreateIssueRequest, IssuesService } from '../../services/issues.service';

@Component({
  selector: 'app-issue-form-page',
  imports: [FormsModule, RouterLink],
  templateUrl: './issue-form-page.component.html',
  styleUrl: './issue-form-page.component.css'
})
export class IssueFormPageComponent {
  private readonly issuesService = inject(IssuesService);
  private readonly router = inject(Router);

  readonly issueTypeOptions: IssueType[] = ['QUESTION', 'BUG', 'DOCUMENTATION', 'FEATURE'];
  readonly issuePriorityOptions: IssuePriority[] = ['LOW', 'MEDIUM', 'HIGH', 'CRITICAL'];

  title = '';
  description = '';
  type: IssueType = 'BUG';
  priority: IssuePriority | '' = '';
  suggestedAssignee: SuggestedAssignee | null = null;
  suggestionMessage = '';
  isLoadingSuggestion = true;
  isSubmitting = false;
  errorMessage = '';

  constructor() {
    this.loadSuggestedAssignee();
  }

  submit(): void {
    if (this.isSubmitting) {
      return;
    }

    this.errorMessage = '';
    this.isSubmitting = true;

    const request: CreateIssueRequest = {
      title: this.title,
      description: this.description,
      type: this.type,
      priority: this.priority || null,
      assignedToId: this.suggestedAssignee?.userId ?? null
    };

    this.issuesService.createIssue(request)
      .pipe(finalize(() => {
        this.isSubmitting = false;
      }))
      .subscribe({
        next: issue => {
          this.router.navigate(['/issues', issue.id]);
        },
        error: error => {
          this.errorMessage = this.getCreateErrorMessage(error);
        }
      });
  }

  private loadSuggestedAssignee(): void {
    this.issuesService.suggestAssignee()
      .pipe(finalize(() => {
        this.isLoadingSuggestion = false;
      }))
      .subscribe({
        next: suggestion => {
          this.suggestedAssignee = suggestion;
          this.suggestionMessage = '';
        },
        error: error => {
          this.suggestedAssignee = null;
          this.suggestionMessage = this.getSuggestionMessage(error);
        }
      });
  }

  private getSuggestionMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 404) {
        return this.getApiMessage(error) || 'Non ci sono utenti assegnabili in questo momento.';
      }
    }

    return 'Non e stato possibile recuperare il suggerimento assegnatario.';
  }

  private getCreateErrorMessage(error: unknown): string {
    if (error instanceof HttpErrorResponse) {
      if (error.status === 403) {
        return 'Il backend ha rifiutato la creazione della issue.';
      }

      if (error.status === 400) {
        return this.getValidationMessage(error) || 'I dati inseriti non sono validi.';
      }

      if (error.status === 401) {
        return 'Devi effettuare di nuovo il login prima di creare una issue.';
      }
    }

    return 'Non e stato possibile creare la issue.';
  }

  private getValidationMessage(error: HttpErrorResponse): string | null {
    const validationErrors = error.error?.errors;

    if (!validationErrors || typeof validationErrors !== 'object') {
      return this.getApiMessage(error);
    }

    for (const value of Object.values(validationErrors)) {
      if (Array.isArray(value) && value.length > 0 && typeof value[0] === 'string') {
        return value[0];
      }
    }

    return this.getApiMessage(error);
  }

  private getApiMessage(error: HttpErrorResponse): string | null {
    if (error.error && typeof error.error.message === 'string') {
      return error.error.message;
    }

    return null;
  }
}
