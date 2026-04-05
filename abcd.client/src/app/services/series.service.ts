import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface SeriesSummary {
  seriesId: number;
  blogId: number;
  title: string;
  description?: string;
  status: string;
  pathSegment?: string;
  dateLastPublished?: string;
  version: string;
  postCount: number;
}

export interface SeriesDetail {
  seriesId: number;
  blogId: number;
  title: string;
  description?: string;
  status: string;
  pathSegment?: string;
  dateLastPublished?: string;
  version: string;
  posts: SeriesPost[];
}

export interface SeriesPost {
  postId: number;
  title: string;
  status: string;
  pathSegment?: string;
  position: number;
}

@Injectable({ providedIn: 'root' })
export class SeriesService {
  constructor(private http: HttpClient) {}

  getAll(): Observable<SeriesSummary[]> {
    return this.http.get<SeriesSummary[]>('/api/series');
  }

  getById(seriesId: number): Observable<SeriesDetail> {
    return this.http.get<SeriesDetail>(`/api/series/${seriesId}`);
  }

  create(title: string, path: string, description?: string): Observable<SeriesDetail> {
    return this.http.post<SeriesDetail>('/api/series', { title, path, description });
  }

  update(seriesId: number, title: string, pathSegment: string, version: string, description?: string): Observable<SeriesDetail> {
    return this.http.put<SeriesDetail>(`/api/series/${seriesId}`, { title, description, pathSegment, version });
  }

  toggleStatus(seriesId: number, version: string): Observable<SeriesDetail> {
    return this.http.post<SeriesDetail>(`/api/series/${seriesId}/status`, { version });
  }

  addPost(seriesId: number, postId: number, position: number, version: string): Observable<SeriesDetail> {
    return this.http.post<SeriesDetail>(`/api/series/${seriesId}/posts`, { postId, position, version });
  }

  removePost(seriesId: number, postId: number, version: string): Observable<SeriesDetail> {
    return this.http.delete<SeriesDetail>(`/api/series/${seriesId}/posts`, { body: { postId, version } });
  }
}
