import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { Fragment } from '../models/fragment.model';
import { IFragmentComponent } from '../models/fragment-component.interface';
import { EditableCode } from '../../models/editable-code.model';

@Component({
  selector: 'app-code-fragment',
  templateUrl: './code-fragment.component.html',
  styleUrls: ['./code-fragment.component.scss'],
  standalone: false
})
export class CodeFragmentComponent implements IFragmentComponent, OnInit, OnChanges {
  @Input() fragment!: Fragment;
  editableCode: EditableCode = new EditableCode();
  isEditing: boolean = false;

  //https://microsoft.github.io/monaco-editor/docs.html
  editorOptions = {
    theme: 'vs-dark',
    language: 'javascript',
    automaticLayout: true
  };

  languages = [
    { value: 'javascript', label: 'JavaScript' },
    { value: 'csharp', label: 'C#' },
    { value: 'html', label: 'HTML' },
    { value: 'python', label: 'Python' },
    { value: 'sql', label: 'SQL' },
    { value: 'typescript', label: 'TypeScript' }
  ];

  ngOnInit() {
    this.initFromFragment();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['fragment']) {
      this.initFromFragment();
    }
  }

  private initFromFragment() {
    if (this.fragment && this.fragment.content) {
      try {
        this.editableCode = EditableCode.fromJSON(this.fragment.content);
      } catch {
        this.editableCode = new EditableCode();
      }
    } else {
      this.editableCode = new EditableCode();
    }
    this.updateEditorOptions();
  }

  setEditMode(isEditing: boolean) {
    this.isEditing = isEditing;
    if (isEditing) {
      this._original = this.editableCode.toJSON();
    }
  }

  private _original: string = '';

  revert() {
    if (this._original) {
      this.editableCode = EditableCode.fromJSON(this._original);
      this.updateEditorOptions();
    }
  }

  onLanguageChange() {
    this.updateEditorOptions();
  }

  private updateEditorOptions() {
    this.editorOptions = {
      ...this.editorOptions,
      language: this.editableCode.language
    };
  }

  getCurrentFragment(): Fragment {
    this.fragment.content = this.editableCode.toJSON();
    return this.fragment;
  }
}
