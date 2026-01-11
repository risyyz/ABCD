import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { EditorRoutingModule } from './editor-routing.module';
import { DashboardComponent } from './dashboard/dashboard.component';
import { CreatePostComponent } from './create-post/create-post.component';
import { EditPostComponent } from './edit-post/edit-post.component';
import { EditableComponent } from './editable/editable.component';
import { RichTextFragmentComponent } from './rich-text-fragment/rich-text-fragment.component';
import { NgxEditorModule } from 'ngx-editor';

@NgModule({
  declarations: [
    DashboardComponent,
    EditPostComponent,
    RichTextFragmentComponent,
    EditableComponent
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
