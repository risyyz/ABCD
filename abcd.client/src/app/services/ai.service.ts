import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface ChatMessage {
  role: 'user' | 'assistant';
  content: string;
}

export interface PostProposal {
  title: string;
  path: string;
  fragments: ProposedFragment[];
}

export interface ProposedFragment {
  fragmentType: 'RichText' | 'Code' | 'Table' | 'Image';
  content: string;
}

@Injectable({ providedIn: 'root' })
export class AiService {
  constructor(private http: HttpClient) {}

  chat(messages: ChatMessage[]): Observable<{ message: string }> {
    return this.http.post<{ message: string }>('/api/ai/chat', { messages }, { withCredentials: true });
  }

  generatePost(messages: ChatMessage[]): Observable<PostProposal> {
    return this.http.post<PostProposal>('/api/ai/generate-post', { messages }, { withCredentials: true });
  }

  createPost(proposal: PostProposal): Observable<{ postId: number }> {
    return this.http.post<{ postId: number }>('/api/ai/create-post', proposal, { withCredentials: true });
  }
}
