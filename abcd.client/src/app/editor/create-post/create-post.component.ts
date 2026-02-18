import { Component, Output, EventEmitter } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-create-post',
  templateUrl: './create-post.component.html',
  standalone: true,
  imports: [FormsModule]
})
export class CreatePostComponent {
  @Output() close = new EventEmitter<void>();
  title: string = '';
  path: string = '';
  errorMessage: string = '';

  constructor(private http: HttpClient) {}

  submit() {
    this.errorMessage = "";
    if (this.title && this.path) {
      this.http.post('/api/posts', {
        blogId: 1, // TODO: Replace with actual blogId as needed
        title: this.title,
        path: this.path
      }).subscribe({
        next: () => this.close.emit(),
        error: err => {
          this.errorMessage = err?.error?.error;
        }
      });
    }
  }
}
