import { Routes } from '@angular/router';
import { ArchivedIssuesPageComponent } from './pages/archived-issues-page/archived-issues-page.component';
import { IssueDetailPageComponent } from './pages/issue-detail-page/issue-detail-page.component';
import { IssueFormPageComponent } from './pages/issue-form-page/issue-form-page.component';
import { IssuesPageComponent } from './pages/issues-page/issues-page.component';
import { LoginPageComponent } from './pages/login-page/login-page.component';
import { NotificationsPageComponent } from './pages/notifications-page/notifications-page.component';
import { UsersPageComponent } from './pages/users-page/users-page.component';

export const routes: Routes = [
  { path: 'login', component: LoginPageComponent },
  { path: 'issues', component: IssuesPageComponent },
  { path: 'issues/new', component: IssueFormPageComponent },
  { path: 'issues/archived', component: ArchivedIssuesPageComponent },
  { path: 'issues/:id', component: IssueDetailPageComponent },
  { path: 'users', component: UsersPageComponent },
  { path: 'notifications', component: NotificationsPageComponent },
  { path: '', pathMatch: 'full', redirectTo: 'issues' },
  { path: '**', redirectTo: 'issues' }
];
