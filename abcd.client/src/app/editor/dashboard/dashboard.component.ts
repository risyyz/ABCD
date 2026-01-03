import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { AuthService } from '../../auth/services/auth.service';
import { HttpClient } from '@angular/common/http';
import { PostService } from '../../services/post.service';

interface Post {
  postId: number; // or postId: number;
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
export class DashboardComponent implements OnInit {
  searchTerm: string = '';
  showCreatePost = false;
  posts: Post[] = [];

  constructor(
    private authService: AuthService,
    private router: Router,
    private http: HttpClient,
    private postService: PostService
  ) {}

  ngOnInit(): void {
    this.fetchPosts();
  }

  fetchPosts(): void {
    this.postService.getPosts().subscribe(posts => {
      this.posts = posts.map(post => ({
        postId: post.postId, // or postId: post.postId,
        title: post.title,
        status: post.status,
        datePublished: post.dateLastPublished ? new Date(post.dateLastPublished) : undefined,
        pathSegment: post.pathSegment ? { value: post.pathSegment } : undefined
      }));
    });
  }

  get filteredPosts(): Post[] {
    const term = this.searchTerm.trim().toLowerCase();
    if (!term) return this.posts;
    return this.posts.filter(post => post.title.toLowerCase().includes(term));
  }

  logout(): void {
    this.authService.signOut();
    this.router.navigate(['/auth/login']);
  }

  openCreatePost(): void {
    this.showCreatePost = true;
  }

  closeCreatePost(): void {
    this.showCreatePost = false;
    this.fetchPosts(); // Refresh posts after closing create modal
  }
}
