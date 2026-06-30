import { AsyncPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { IssuesService } from '../../services/issues.service';

@Component({
  selector: 'app-archived-issues-page',
  imports: [AsyncPipe],
  templateUrl: './archived-issues-page.component.html',
  styleUrl: './archived-issues-page.component.css'
})
export class ArchivedIssuesPageComponent {
  private readonly issuesService = inject(IssuesService);
  archivedIssues$ = this.issuesService.getArchivedIssues();
}
