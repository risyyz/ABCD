import { Component, OnInit, OnDestroy } from '@angular/core';
import { NgxEditorComponent, NgxEditorMenuComponent, Editor, Toolbar } from 'ngx-editor';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-rich-text-fragment',
  templateUrl: './rich-text-fragment.component.html',
  styleUrls: ['./rich-text-fragment.component.scss'],
  standalone:false
})
export class RichTextFragmentComponent implements OnInit, OnDestroy {
  html = '';
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
}
