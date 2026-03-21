import { Component, Input } from '@angular/core';
import { PublicFragment } from '../../services/post.service';

@Component({
  selector: 'app-public-richtext-fragment',
  templateUrl: './public-richtext-fragment.component.html',
  standalone: false
})
export class PublicRichTextFragmentComponent {
  @Input() fragment!: PublicFragment;
}
