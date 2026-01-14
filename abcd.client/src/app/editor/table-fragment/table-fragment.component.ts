import { Component, Input } from '@angular/core';
import { Fragment } from '../models/fragment.model';

@Component({
  selector: 'app-table-fragment',
  templateUrl: './table-fragment.component.html',
  styleUrls: ['./table-fragment.component.scss'],
  standalone: false
})
export class TableFragmentComponent {
  @Input() fragment!: Fragment;
  // Add table logic and properties here as needed
  columns: any = 5;
  rows: any = 6
}
