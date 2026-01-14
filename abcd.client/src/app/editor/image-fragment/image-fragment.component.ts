import { Component, Input } from '@angular/core';
import { Fragment } from '../models/fragment.model';

@Component({
  selector: 'app-image-fragment',
  templateUrl: './image-fragment.component.html',
  styleUrls: ['./image-fragment.component.scss'],
  standalone: false
})
export class ImageFragmentComponent {
  @Input() fragment!: Fragment;
  imageSrc: string | ArrayBuffer | null = null;
  caption: string = '';

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const reader = new FileReader();
      reader.onload = e => {
        this.imageSrc = reader.result;
      };
      reader.readAsDataURL(input.files[0]);
    }
  }
}
