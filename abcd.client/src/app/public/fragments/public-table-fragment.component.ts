import { Component, Input, OnInit } from '@angular/core';
import { PublicFragment } from '../../services/post.service';
import { EditableTable } from '../../models/editable-table.model';

@Component({
  selector: 'app-public-table-fragment',
  templateUrl: './public-table-fragment.component.html',
  standalone: false
})
export class PublicTableFragmentComponent implements OnInit {
  @Input() fragment!: PublicFragment;
  table: EditableTable = new EditableTable();

  ngOnInit(): void {
    if (this.fragment?.content) {
      try {
        this.table = EditableTable.fromJSON(this.fragment.content);
      } catch {
        this.table = new EditableTable();
      }
    }
  }
}
