import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { SeriesService, SeriesDetail, SeriesPost } from '../../services/series.service';
import { PostService } from '../../services/post.service';

@Component({
  selector: 'app-edit-series',
  standalone: false,
  templateUrl: './edit-series.component.html',
  styleUrl: './edit-series.component.css'
})
export class EditSeriesComponent implements OnInit {
  series: SeriesDetail | null = null;
  errorMessage: string = '';
  isEditing = false;

  editTitle: string = '';
  editDescription: string = '';
  editPathSegment: string = '';

  availablePosts: { postId: number; title: string }[] = [];
  addPostId: number | null = null;
  addPostPosition: number = 1;
  showAddPost = false;

  constructor(
    private route: ActivatedRoute,
    private seriesService: SeriesService,
    private postService: PostService
  ) {}

  ngOnInit(): void {
    const seriesId = Number(this.route.snapshot.paramMap.get('seriesId'));
    this.loadSeries(seriesId);
  }

  loadSeries(seriesId: number): void {
    this.seriesService.getById(seriesId).subscribe({
      next: series => {
        this.series = series;
        this.errorMessage = '';
      },
      error: () => this.errorMessage = 'Failed to load series.'
    });
  }

  startEdit(): void {
    if (!this.series) return;
    this.editTitle = this.series.title;
    this.editDescription = this.series.description || '';
    this.editPathSegment = this.series.pathSegment || '';
    this.isEditing = true;
  }

  cancelEdit(): void {
    this.isEditing = false;
  }

  saveEdit(): void {
    if (!this.series) return;
    this.seriesService.update(
      this.series.seriesId,
      this.editTitle,
      this.editPathSegment,
      this.series.version,
      this.editDescription || undefined
    ).subscribe({
      next: series => {
        this.series = series;
        this.isEditing = false;
        this.errorMessage = '';
      },
      error: err => this.errorMessage = err?.error?.error || 'Failed to save.'
    });
  }

  toggleStatus(): void {
    if (!this.series) return;
    this.seriesService.toggleStatus(this.series.seriesId, this.series.version).subscribe({
      next: series => {
        this.series = series;
        this.errorMessage = '';
      },
      error: err => this.errorMessage = err?.error?.error || 'Failed to toggle status.'
    });
  }

  openAddPost(): void {
    this.postService.getPosts().subscribe(posts => {
      const assignedIds = new Set(this.series?.posts.map(p => p.postId) || []);
      this.availablePosts = posts
        .filter((p: any) => !assignedIds.has(p.postId))
        .map((p: any) => ({ postId: p.postId, title: p.title }));
      this.addPostPosition = (this.series?.posts.length || 0) + 1;
      this.addPostId = null;
      this.showAddPost = true;
    });
  }

  closeAddPost(): void {
    this.showAddPost = false;
  }

  submitAddPost(): void {
    if (!this.series || !this.addPostId) return;
    this.seriesService.addPost(this.series.seriesId, this.addPostId, this.addPostPosition, this.series.version).subscribe({
      next: series => {
        this.series = series;
        this.showAddPost = false;
        this.errorMessage = '';
      },
      error: err => this.errorMessage = err?.error?.error || 'Failed to add post.'
    });
  }

  removePost(post: SeriesPost): void {
    if (!this.series) return;
    this.seriesService.removePost(this.series.seriesId, post.postId, this.series.version).subscribe({
      next: series => {
        this.series = series;
        this.errorMessage = '';
      },
      error: err => this.errorMessage = err?.error?.error || 'Failed to remove post.'
    });
  }
}
