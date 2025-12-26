import { Component, Output, EventEmitter } from '@angular/core';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-create-post',
  templateUrl: './create-post.component.html',
  standalone: true,
  imports: [FormsModule]
})
export class CreatePostComponent {
  @Output() close = new EventEmitter<void>();
  title = '';
  path = '';

  submit() {
    if (this.title && this.path) {
      // TODO: handle post creation logic here
      this.close.emit();
    }
  }
}
