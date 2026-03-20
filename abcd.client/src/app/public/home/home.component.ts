import { Component, OnInit } from '@angular/core';
import { PostService, PostSummary } from '../../services/post.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  standalone: false
})
export class HomeComponent implements OnInit {
  posts: PostSummary[] = [];
  isLoading = false;
  error: string | null = null;

  constructor(private postService: PostService) {}

  ngOnInit(): void {
    this.isLoading = true;
    this.postService.getPublishedPosts().subscribe({
      next: posts => { this.posts = posts; this.isLoading = false; },
      error: _ => { this.error = 'Failed to load posts.'; this.isLoading = false; }
    });
  }
}
