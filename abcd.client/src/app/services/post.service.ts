import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Post } from '../editor/models/post.model'; // adjust path as needed

@Injectable({ providedIn: 'root' })
export class PostService {
  constructor(private http: HttpClient) {}

  getPosts(): Observable<any[]> {
    return this.http.get<any[]>('/api/posts');
  }

  getPost(postId: number): Observable<Post> {
    return this.http.get<Post>(`/api/posts/${postId}`);
  }

  updateFragmentPosition(postId: number, position: number, newPosition: number) {
    return this.http.put(`/api/posts/${postId}/fragments/${position}/position`, { newPosition });
  }

  // You can add more methods for create, update, delete as needed
}
