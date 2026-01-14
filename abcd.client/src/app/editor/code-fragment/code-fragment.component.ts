import { Component, Input, OnDestroy } from '@angular/core';
import { Editor, Toolbar } from 'ngx-editor';
import { Fragment } from '../models/fragment.model'; // Adjust the path as needed

@Component({
  selector: 'app-code-fragment',
  templateUrl: './code-fragment.component.html',
  styleUrls: ['./code-fragment.component.scss'],
  standalone: false
})
export class CodeFragmentComponent implements OnDestroy {
  @Input() fragment!: Fragment;
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

  ngOnDestroy(): void {
    this.editor.destroy();
  }
}
