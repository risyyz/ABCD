import { Component, Input, OnInit, OnDestroy } from '@angular/core';
import { NgxEditorComponent, NgxEditorMenuComponent, Editor, Toolbar } from 'ngx-editor';
import { FormsModule } from '@angular/forms';
import { Fragment } from '../models/fragment.model';
import { IFragmentComponent } from '../models/fragment-component.interface';

@Component({
  selector: 'app-rich-text-fragment',
  templateUrl: './rich-text-fragment.component.html',
  styleUrls: ['./rich-text-fragment.component.scss'],
  standalone:false
})
export class RichTextFragmentComponent implements OnInit, OnDestroy, IFragmentComponent {
  @Input() fragment!: Fragment;
  editor!: Editor;
  toolbar: Toolbar = [
    [{ heading: ['h1', 'h2', 'h3', 'h4', 'h5', 'h6'] }],
    ['bold', 'italic', 'underline', 'strike'],
    // ... other desired toolbar items ...
    ['link', 'image'],
    ['text_color', 'background_color'],
    ['code', 'blockquote'],
    ['align_left', 'align_center', 'align_right', 'align_justify'],
    ['superscript', 'subscript'],
    ['undo', 'redo'],
    // ... other desired toolbar items ...
  ];

  ngOnInit(): void {
    this.editor = new Editor();
  }
  
  ngOnDestroy(): void {
    this.editor.destroy();
  }

  setEditMode(isEditing: boolean) {
    console.log('RichTextFragmentComponent setEditMode: ' + isEditing);
  }

  revert() {
    console.log('reverting rich text fragment to original');
  }

  getCurrentFragment(): Fragment {
    console.log('returning latest rich text fragment');
    return this.fragment;
  }
}
