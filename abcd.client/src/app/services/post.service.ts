import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Post } from '../editor/models/post.model'; 
import { FragmentPositionChangeRequest } from '../editor/models/fragment-position-change-request.model'; 

@Injectable({ providedIn: 'root' })
export class PostService {
  constructor(private http: HttpClient) {}

  getPosts(): Observable<any[]> {
    return this.http.get<any[]>('/api/posts');
  }

  getPost(postId: number): Observable<Post> {
    return this.http.get<Post>(`/api/posts/${postId}`);
  }

  updateFragmentPosition(request: FragmentPositionChangeRequest) {
    return this.http.patch(
      `/api/posts/${request.postId}/fragments/${request.fragmentId}/position`,
      { newPosition: request.newPosition, version: request.version }
    );
  }
  // You can add more methods for create, update, delete as needed
}
