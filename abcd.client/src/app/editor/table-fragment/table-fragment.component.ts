import { Component, Input } from '@angular/core';
import { Fragment } from '../models/fragment.model';
import { IFragmentComponent } from '../models/fragment-component.interface';

@Component({
  selector: 'app-table-fragment',
  templateUrl: './table-fragment.component.html',
  styleUrls: ['./table-fragment.component.scss'],
  standalone: false
})
export class TableFragmentComponent implements IFragmentComponent {
  @Input() fragment!: Fragment;
  // Add table logic and properties here as needed
  columns: any = 5;
  rows: any = 6

  setEditMode(isEditing: boolean) {
    console.log('TableFragmentComponent setEditMode: ' + isEditing);
  }

  revert() {
    console.log('reverting table fragment to original');
  }

  getCurrentFragment(): Fragment {
    console.log('returning latest table fragment');
    return this.fragment;
  }
}
