import { Component, Input } from '@angular/core';
import { Fragment } from '../models/fragment.model';
import { IFragmentComponent } from '../models/fragment-component.interface';


@Component({
  selector: 'app-code-fragment',
  templateUrl: './code-fragment.component.html',
  styleUrls: ['./code-fragment.component.scss'],
  standalone: false
})
export class CodeFragmentComponent implements IFragmentComponent {
  @Input() fragment!: Fragment;
  code: string = "function x() {\nconsole.log('Hello world!');\n}";
  language: string = 'javascript';
  isEditing: boolean = false;

  languages = [
    { value: 'javascript', label: 'JavaScript' },
    { value: 'csharp', label: 'C#' },
    { value: 'java', label: 'Java' },
    { value: 'html', label: 'HTML' }
  ];

  setEditMode(isEditing: boolean) {
    this.isEditing = isEditing;
  }

  revert() {
    // Implement revert logic if needed
  }

  getCurrentFragment(): Fragment {
    console.log(this.code);
    return this.fragment;
  }
}
