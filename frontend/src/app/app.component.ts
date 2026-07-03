import { Component } from '@angular/core';
import { NavigationEnd, Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { filter } from 'rxjs';
import { User } from './models/user.model';
import { AuthService } from './services/auth.service';

@Component({
  selector: 'app-root',
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  templateUrl: './app.component.html',
  styleUrl: './app.component.css'
})
export class AppComponent {
  title = 'BugBoard26';
  isLoginPage = false;
  currentUser: User | null = null;

  constructor(
    private readonly authService: AuthService,
    private readonly router: Router
  ) {
    this.refreshHeaderState();

    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(() => this.refreshHeaderState());
  }

  logout(): void {
    this.authService.logout();
    this.router.navigate(['/login']);
  }

  private refreshHeaderState(): void {
    this.isLoginPage = this.router.url.startsWith('/login');
    this.currentUser = this.authService.getCurrentUser();
  }
}
