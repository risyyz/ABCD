import { Component, Input, OnInit } from '@angular/core';
import { PublicFragment } from '../../services/post.service';
import { EditableCode } from '../../models/editable-code.model';

@Component({
  selector: 'app-public-code-fragment',
  templateUrl: './public-code-fragment.component.html',
  standalone: false
})
export class PublicCodeFragmentComponent implements OnInit {
  @Input() fragment!: PublicFragment;
  code: EditableCode = new EditableCode();

  ngOnInit(): void {
    if (this.fragment?.content) {
      try {
        this.code = EditableCode.fromJSON(this.fragment.content);
      } catch {
        this.code = new EditableCode();
      }
    }
  }
}
