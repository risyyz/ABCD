import { Component, Input, OnInit } from '@angular/core';
import { PublicFragment } from '../../services/post.service';
import { EditableImage } from '../../models/editable-image.model';

@Component({
  selector: 'app-public-image-fragment',
  templateUrl: './public-image-fragment.component.html',
  standalone: false
})
export class PublicImageFragmentComponent implements OnInit {
  @Input() fragment!: PublicFragment;
  image: EditableImage = new EditableImage();

  ngOnInit(): void {
    if (this.fragment?.content) {
      this.image = EditableImage.fromJSON(this.fragment.content);
    }
  }
}
