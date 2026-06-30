import { AsyncPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { map, switchMap } from 'rxjs';
import { IssuesService } from '../../services/issues.service';

@Component({
  selector: 'app-issue-detail-page',
  imports: [AsyncPipe, RouterLink],
  templateUrl: './issue-detail-page.component.html',
  styleUrl: './issue-detail-page.component.css'
})
export class IssueDetailPageComponent {
  private readonly route = inject(ActivatedRoute);
  private readonly issuesService = inject(IssuesService);

  issue$ = this.route.paramMap.pipe(
    map(params => Number(params.get('id'))),
    switchMap(id => this.issuesService.getIssueById(id))
  );
}
