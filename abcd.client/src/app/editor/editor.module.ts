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
import { AiChatComponent } from './ai-chat/ai-chat.component';
import { SeriesListComponent } from './series-list/series-list.component';
import { EditSeriesComponent } from './edit-series/edit-series.component';
import { CreateSeriesComponent } from './create-series/create-series.component';
import { MonacoEditorModule } from 'ngx-monaco-editor-v2';

@NgModule({
  declarations: [
    AiChatComponent,
    CodeFragmentComponent,
    DashboardComponent,
    EditPostComponent,
    EditableFragmentComponent,
    EditableGenericComponent,
    ImageFragmentComponent,
    RichTextFragmentComponent,
    SeriesListComponent,
    EditSeriesComponent,
    TableFragmentComponent,
  ],
  imports: [
    CommonModule,
    FormsModule,
    EditorRoutingModule,
    CreatePostComponent,
    CreateSeriesComponent,
    NgxEditorModule,
    MonacoEditorModule.forRoot()
  ]
})
export class EditorModule { }
