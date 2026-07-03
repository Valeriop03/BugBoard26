import { AsyncPipe, DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { catchError, map, of, startWith, switchMap } from 'rxjs';
import { Issue } from '../../models/issue.model';
import { IssuesService } from '../../services/issues.service';

interface IssueDetailState {
  issue: Issue | null;
  isLoading: boolean;
  notFound: boolean;
  errorMessage: string;
}

@Component({
  selector: 'app-issue-detail-page',
  imports: [AsyncPipe, DatePipe, RouterLink],
  templateUrl: './issue-detail-page.component.html',
  styleUrl: './issue-detail-page.component.css'
})
export class IssueDetailPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly issuesService = inject(IssuesService);

  readonly state$ = this.route.paramMap.pipe(
    map(params => Number(params.get('id'))),
    switchMap(id => {
      if (!Number.isInteger(id) || id <= 0) {
        return of<IssueDetailState>({
          issue: null,
          isLoading: false,
          notFound: true,
          errorMessage: ''
        });
      }

      return this.issuesService.getIssueById(id).pipe(
        map(issue => ({
          issue,
          isLoading: false,
          notFound: false,
          errorMessage: ''
        })),
        startWith({
          issue: null,
          isLoading: true,
          notFound: false,
          errorMessage: ''
        }),
        catchError(error => of(this.buildErrorState(error)))
      );
    })
  );

  private buildErrorState(error: unknown): IssueDetailState {
    if (error instanceof HttpErrorResponse && error.status === 404) {
      return {
        issue: null,
        isLoading: false,
        notFound: true,
        errorMessage: ''
      };
    }

    return {
      issue: null,
      isLoading: false,
      notFound: false,
      errorMessage: 'Non e stato possibile caricare il dettaglio della issue.'
    };
  }
}
