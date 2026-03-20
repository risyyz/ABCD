import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { PostService, PublicPostDetail } from '../../services/post.service';

@Component({
  selector: 'app-post-detail',
  templateUrl: './post-detail.component.html',
  standalone: false
})
export class PostDetailComponent implements OnInit {
  post: PublicPostDetail | null = null;
  isLoading = false;
  error: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private postService: PostService
  ) {}

  ngOnInit(): void {
    const pathSegment = this.route.snapshot.paramMap.get('pathSegment') ?? '';
    this.isLoading = true;
    this.postService.getPublishedPost(pathSegment).subscribe({
      next: post => { this.post = post; this.isLoading = false; },
      error: _ => { this.error = 'Post not found.'; this.isLoading = false; }
    });
  }
}
