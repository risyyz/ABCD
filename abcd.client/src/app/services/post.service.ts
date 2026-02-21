import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Post } from '../editor/models/post.model'; 
import { MoveFragmentRequest } from '../editor/models/move-fragment-request.model'; 
import { FragmentUpdateRequest } from '../editor/models/fragment-update-request.model';

export interface ImageUploadResponse {
  imageUrl: string;
  fileName: string;
}

@Injectable({ providedIn: 'root' })
export class PostService {
  constructor(private http: HttpClient) {}

  getPosts(): Observable<any[]> {
    return this.http.get<any[]>('/api/posts');
  }

  getPost(postId: number): Observable<Post> {
    return this.http.get<Post>(`/api/posts/${postId}`);
  }

  moveFragment(request: MoveFragmentRequest): Observable<Post> {
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

  deleteFragment(postId: number, fragmentId: number, version: string): Observable<Post> {
    return this.http.delete<Post>(
      `/api/posts/${postId}/fragments/${fragmentId}`,
      { headers: { 'If-Match': version } }
    );
  }

  saveFragment(request: FragmentUpdateRequest): Observable<Post> {
    return this.http.put<Post>(
      `/api/posts/${request.postId}/fragments/${request.fragmentId}`,
      { content: request.content, version: request.version }
    );
  }

  uploadImage(postId: number, file: File, destinationFileName: string): Observable<ImageUploadResponse> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('destinationFileName', destinationFileName);

    return this.http.post<ImageUploadResponse>(`/api/file/posts/${postId}/image`, formData);
  }
}
