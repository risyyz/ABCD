import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { Editor, Toolbar } from 'ngx-editor';
import { Fragment } from '../models/fragment.model';
import { IFragmentComponent } from '../models/fragment-component.interface';

@Component({
  selector: 'app-code-fragment',
  templateUrl: './code-fragment.component.html',
  styleUrls: ['./code-fragment.component.scss'],
  standalone: false
})
export class CodeFragmentComponent implements OnInit, OnDestroy, IFragmentComponent {
  @Input() fragment!: Fragment;
  original!: Fragment;
  isEditable: boolean = false;

  language: string = 'javascript';
  editor: Editor;
  toolbar: Toolbar = [
    ['code']
  ];
  languages = [
    { value: 'javascript', label: 'JavaScript' },
    { value: 'csharp', label: 'C#' },
    { value: 'java', label: 'Java' },
    { value: 'html', label: 'HTML' }
  ];

  constructor() {
    this.editor = new Editor();
  }

  ngOnInit() {
    this.original = this.deepCopy(this.fragment);
  }

  ngOnDestroy(): void {
    this.editor.destroy();
  }

  setEditMode(isEditing: boolean) {
    console.log('CodeFragmentComponent setEditMode: ' + isEditing);
    this.isEditable = isEditing;
  }

  revert() {
    console.log('reverting code fragment to original');
    Object.assign(this.fragment, this.original);
  }

  getCurrentFragment(): Fragment {
    console.log('returning latest code fragment');
    return this.fragment;
  }

  deepCopy(fragment: Fragment): Fragment {
    return JSON.parse(JSON.stringify(fragment));
  }
}
