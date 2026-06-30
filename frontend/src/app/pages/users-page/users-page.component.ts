import { AsyncPipe } from '@angular/common';
import { Component, inject } from '@angular/core';
import { UsersService } from '../../services/users.service';

@Component({
  selector: 'app-users-page',
  imports: [AsyncPipe],
  templateUrl: './users-page.component.html',
  styleUrl: './users-page.component.css'
})
export class UsersPageComponent {
  private readonly usersService = inject(UsersService);
  users$ = this.usersService.getUsers();
}
