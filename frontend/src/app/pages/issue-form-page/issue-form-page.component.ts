import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-issue-form-page',
  imports: [FormsModule],
  templateUrl: './issue-form-page.component.html',
  styleUrl: './issue-form-page.component.css'
})
export class IssueFormPageComponent {
  title = '';
  description = '';
  type = 'BUG';
  priority = '';
}
