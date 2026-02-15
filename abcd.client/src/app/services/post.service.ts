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

  updateFragmentPosition(request: FragmentPositionChangeRequest): Observable<Post> {
    return this.http.patch<Post>(
      `/api/posts/${request.postId}/fragments/${request.fragmentId}/position`,
      { newPosition: request.newPosition, version: request.version }
    );
  }

  addFragment(postId: number, afterFragmentId: number, fragmentType: string, version: string): Observable<Post> {
    return this.http.post<Post>(
      `/api/posts/${postId}/fragments`,
      { afterFragmentId, fragmentType, version }
    );
  }
  // You can add more methods for create, update, delete as needed
}
