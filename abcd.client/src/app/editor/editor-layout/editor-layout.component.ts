import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../auth/services/auth.service';

@Component({
  selector: 'app-editor-layout',
  standalone: false,
  templateUrl: './editor-layout.component.html'
})
export class EditorLayoutComponent {
  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  logout(): void {
    this.authService.signOut();
    this.router.navigate(['/auth/login']);
  }
}
