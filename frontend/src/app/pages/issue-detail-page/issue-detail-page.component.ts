import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import { Issue, IssueStatus } from '../../models/issue.model';
import { AuthService } from '../../services/auth.service';
import { IssuesService } from '../../services/issues.service';

@Component({
  selector: 'app-issue-detail-page',
  imports: [DatePipe, FormsModule, RouterLink],
  templateUrl: './issue-detail-page.component.html',
  styleUrl: './issue-detail-page.component.css'
})
export class IssueDetailPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);
  private readonly issuesService = inject(IssuesService);
  private readonly authService = inject(AuthService);

  readonly statusOptions: IssueStatus[] = ['TODO', 'IN_PROGRESS', 'RESOLVED', 'CLOSED'];

  issue: Issue | null = null;
  selectedStatus: IssueStatus = 'TODO';
  duplicateOfIssueId: number | null = null;
  isLoading = true;
  isSavingStatus = false;
  isArchiving = false;
  isMarkingDuplicate = false;
  notFound = false;
  errorMessage = '';
  actionMessage = '';
  actionError = '';

  get isAdmin(): boolean {
    return this.authService.getCurrentUser()?.role === 'ADMIN';
  }

  get canChangeStatus(): boolean {
    const user = this.authService.getCurrentUser();

    return !!this.issue && !!user && (
      user.role === 'ADMIN' ||
      (user.role === 'USER' && user.id === this.issue.assignedToId)
    );
  }

  constructor() {
    const id = Number(this.route.snapshot.paramMap.get('id'));

    if (!Number.isInteger(id) || id <= 0) {
      this.isLoading = false;
      this.notFound = true;
      return;
    }

    this.loadIssue(id);
  }

  updateStatus(): void {
    if (!this.canChangeStatus) {
      this.actionError = 'Non hai i permessi per questa operazione.';
      return;
    }

    if (!this.issue || this.isSavingStatus) {
      return;
    }

    this.actionMessage = '';
    this.actionError = '';
    this.isSavingStatus = true;

    this.issuesService.updateStatus(this.issue.id, this.selectedStatus)
      .pipe(finalize(() => {
        this.isSavingStatus = false;
      }))
      .subscribe({
        next: issue => {
          this.issue = issue;
          this.selectedStatus = issue.status;
          this.actionMessage = 'Stato aggiornato.';
        },
        error: error => {
          this.actionError = this.getActionErrorMessage(error, 'Non e stato possibile aggiornare lo stato.');
        }
      });
  }

  archiveIssue(): void {
    if (!this.isAdmin) {
      this.actionError = 'Non hai i permessi per questa operazione.';
      return;
    }

    if (!this.issue || this.isArchiving) {
      return;
    }

    this.actionMessage = '';
    this.actionError = '';
    this.isArchiving = true;

    this.issuesService.archiveIssue(this.issue.id)
      .pipe(finalize(() => {
        this.isArchiving = false;
      }))
      .subscribe({
        next: () => {
          this.router.navigate(['/issues/archived']);
        },
        error: error => {
          this.actionError = this.getActionErrorMessage(error, 'Non e stato possibile archiviare la issue.');
        }
      });
  }

  markAsDuplicate(): void {
    if (!this.isAdmin) {
      this.actionError = 'Non hai i permessi per questa operazione.';
      return;
    }

    if (!this.issue || !this.duplicateOfIssueId || this.isMarkingDuplicate) {
      this.actionError = "Inserisci l'ID della issue originale.";
      return;
    }

    this.actionMessage = '';
    this.actionError = '';
    this.isMarkingDuplicate = true;

    this.issuesService.markAsDuplicate(this.issue.id, this.duplicateOfIssueId)
      .pipe(finalize(() => {
        this.isMarkingDuplicate = false;
      }))
      .subscribe({
        next: issue => {
          this.issue = issue;
          this.selectedStatus = issue.status;
          this.actionMessage = 'Issue segnata come duplicata.';
        },
        error: error => {
          this.actionError = this.getActionErrorMessage(error, 'Non e stato possibile segnare la issue come duplicata.');
        }
      });
  }

  private loadIssue(id: number): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.issuesService.getIssueById(id).subscribe({
      next: issue => {
        this.issue = issue;
        this.selectedStatus = issue.status;
        this.duplicateOfIssueId = issue.duplicateOfIssueId;
        this.isLoading = false;
      },
      error: error => {
        this.isLoading = false;

        if (error instanceof HttpErrorResponse && error.status === 404) {
          this.notFound = true;
          return;
        }

        this.errorMessage = 'Non e stato possibile caricare il dettaglio della issue.';
      }
    });
  }

  private getActionErrorMessage(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse && error.status === 404) {
      return 'Issue non trovata.';
    }

    if (error instanceof HttpErrorResponse && error.status === 403) {
      return 'Non hai i permessi per questa operazione.';
    }

    if (error instanceof HttpErrorResponse && error.error?.message) {
      return error.error.message;
    }

    return fallback;
  }
}
