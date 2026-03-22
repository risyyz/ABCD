import { Component, OnInit, OnDestroy } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Subscription, combineLatest } from 'rxjs';
import { PostService, PostSummary } from '../../services/post.service';

@Component({
  selector: 'app-home',
  templateUrl: './home.component.html',
  standalone: false
})
export class HomeComponent implements OnInit, OnDestroy {
  allPosts: PostSummary[] = [];
  posts: PostSummary[] = [];
  searchQuery = '';
  isLoading = false;
  error: string | null = null;
  private sub: Subscription | null = null;

  constructor(private postService: PostService, private route: ActivatedRoute) {}

  ngOnInit(): void {
    this.isLoading = true;
    this.sub = combineLatest([
      this.postService.getPublishedPosts(),
      this.route.queryParams
    ]).subscribe({
      next: ([posts, params]) => {
        this.allPosts = posts;
        this.searchQuery = params['q'] || '';
        this.applyFilter();
        this.isLoading = false;
      },
      error: _ => { this.error = 'Failed to load posts.'; this.isLoading = false; }
    });
  }

  private applyFilter(): void {
    const q = this.searchQuery.toLowerCase().trim();
    this.posts = q
      ? this.allPosts.filter(p => p.title.toLowerCase().includes(q))
      : this.allPosts;
  }

  ngOnDestroy(): void {
    this.sub?.unsubscribe();
  }
}
