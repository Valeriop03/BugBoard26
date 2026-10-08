import { HttpErrorResponse } from '@angular/common/http';
import { Component, DestroyRef, inject } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { FormsModule, NgForm } from '@angular/forms';
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
  private readonly destroyRef = inject(DestroyRef);

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
  submitAttempted = false;

  get titleError(): string {
    return this.title.trim().length < 3 ? 'Inserisci almeno 3 caratteri.' :
      this.title.length > 200 ? 'Il titolo deve avere al massimo 200 caratteri.' : '';
  }

  get descriptionError(): string {
    return this.description.trim().length < 3 ? 'Inserisci almeno 3 caratteri.' :
      this.description.length > 4000 ? 'La descrizione deve avere al massimo 4000 caratteri.' : '';
  }

  constructor() {
    this.loadSuggestedAssignee();
  }

  submit(form: NgForm): void {
    if (this.isSubmitting) {
      return;
    }

    this.errorMessage = '';
    this.submitAttempted = true;

    if (form.invalid || this.titleError || this.descriptionError) {
      form.control.markAllAsTouched();
      return;
    }

    this.isSubmitting = true;

    const request: CreateIssueRequest = {
      title: this.title.trim(),
      description: this.description.trim(),
      type: this.type,
      priority: this.priority || null,
      assignedToId: this.suggestedAssignee?.userId ?? null
    };

    this.issuesService.createIssue(request)
      .pipe(takeUntilDestroyed(this.destroyRef), finalize(() => {
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
      .pipe(takeUntilDestroyed(this.destroyRef), finalize(() => {
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
