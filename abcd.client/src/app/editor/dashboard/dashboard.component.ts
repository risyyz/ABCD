import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../auth/services/auth.service';

interface Post {
  title: string;
  status: string;
  datePublished?: Date;
}

@Component({
  selector: 'app-dashboard',
  standalone: false,
  templateUrl: './dashboard.component.html',
  styleUrl: './dashboard.component.css'
})
export class DashboardComponent {
  searchTerm: string = '';
  posts: Post[] = [
    // Example data; replace with real data or fetch from a service
    { title: 'First Post', status: 'published', datePublished: new Date() },
    { title: 'Draft Post', status: 'draft' },
    { title: 'Second Post', status: 'published', datePublished: new Date() }
  ];

  constructor(
    private authService: AuthService,
    private router: Router
  ) {}

  get filteredPosts(): Post[] {
    const term = this.searchTerm.trim().toLowerCase();
    if (!term) return this.posts;
    return this.posts.filter(post => post.title.toLowerCase().includes(term));
  }

  logout(): void {
    this.authService.signOut();
    this.router.navigate(['/auth/login']);
  }
}
