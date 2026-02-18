import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { EditorRoutingModule } from './editor-routing.module';
import { DashboardComponent } from './dashboard/dashboard.component';
import { CreatePostComponent } from './create-post/create-post.component';
import { EditPostComponent } from './edit-post/edit-post.component';
import { EditableGenericComponent } from './editable-generic/editable-generic.component';
import { EditableFragmentComponent } from './editable-fragment/editable-fragment.component';
import { RichTextFragmentComponent } from './rich-text-fragment/rich-text-fragment.component';
import { NgxEditorModule } from 'ngx-editor';
import { ImageFragmentComponent } from './image-fragment/image-fragment.component';
import { TableFragmentComponent } from './table-fragment/table-fragment.component';
import { CodeFragmentComponent } from './code-fragment/code-fragment.component';

@NgModule({
  declarations: [
    CodeFragmentComponent,
    DashboardComponent,
    EditPostComponent,
    EditableFragmentComponent,
    EditableGenericComponent,
    ImageFragmentComponent,
    RichTextFragmentComponent,
    TableFragmentComponent,
  ],
  imports: [
    CommonModule,
    FormsModule,
    EditorRoutingModule,
    CreatePostComponent,
    NgxEditorModule
  ]
})
export class EditorModule { }
