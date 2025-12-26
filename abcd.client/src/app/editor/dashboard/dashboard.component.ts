import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../auth/services/auth.service';

interface Post {
  title: string;
  status: string;
  datePublished?: Date;
  pathSegment?: { value: string };
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
    { title: 'First Post', status: 'published', datePublished: new Date(), pathSegment: { value: 'first-post' } },
    { title: 'Draft Post', status: 'draft', pathSegment: { value: 'draft-post' } },
    { title: 'Second Post', status: 'published', datePublished: new Date(), pathSegment: { value: 'second-post' } }
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
