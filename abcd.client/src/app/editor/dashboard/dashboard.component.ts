import { Component, OnInit } from '@angular/core';
import { PostService } from '../../services/post.service';
import { SeriesService, SeriesSummary } from '../../services/series.service';
import { forkJoin } from 'rxjs';

interface Post {
  postId: number;
  title: string;
  status: string;
  datePublished?: Date;
  pathSegment?: { value: string };
  seriesId?: number;
  seriesTitle?: string;
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
    private postService: PostService,
    private seriesService: SeriesService
  ) {}

  ngOnInit(): void {
    this.fetchPosts();
  }

  fetchPosts(): void {
    forkJoin([
      this.postService.getPosts(),
      this.seriesService.getAll()
    ]).subscribe(([posts, seriesList]) => {
      const seriesMap = new Map<number, string>(
        seriesList.map(s => [s.seriesId, s.title])
      );
      this.posts = posts.map(post => ({
        postId: post.postId,
        title: post.title,
        status: post.status,
        datePublished: post.dateLastPublished ? new Date(post.dateLastPublished) : undefined,
        pathSegment: post.pathSegment ? { value: post.pathSegment } : undefined,
        seriesId: post.seriesId ?? undefined,
        seriesTitle: post.seriesId ? seriesMap.get(post.seriesId) : undefined
      }));
    });
  }

  get filteredPosts(): Post[] {
    const term = this.searchTerm.trim().toLowerCase();
    if (!term) return this.posts;
    return this.posts.filter(post => post.title.toLowerCase().includes(term));
  }

  openCreatePost(): void {
    this.showCreatePost = true;
  }

  closeCreatePost(): void {
    this.showCreatePost = false;
    this.fetchPosts(); // Refresh posts after closing create modal
  }
}
