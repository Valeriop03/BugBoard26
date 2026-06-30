import { Component } from '@angular/core';
import { inject } from '@angular/core';
import { AsyncPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import { IssuesService } from '../../services/issues.service';

@Component({
  selector: 'app-issues-page',
  imports: [AsyncPipe, RouterLink],
  templateUrl: './issues-page.component.html',
  styleUrl: './issues-page.component.css'
})
export class IssuesPageComponent {
  private readonly issuesService = inject(IssuesService);
  issues$ = this.issuesService.getIssues();
}
