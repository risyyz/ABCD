import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

import { EditorRoutingModule } from './editor-routing.module';
import { DashboardComponent } from './dashboard/dashboard.component';
import { CreatePostComponent } from './create-post/create-post.component';
import { EditPostComponent } from './edit-post/edit-post.component';

@NgModule({
  declarations: [
    DashboardComponent,
    EditPostComponent
  ],
  imports: [
    CommonModule,
    FormsModule,
    EditorRoutingModule,
    CreatePostComponent
  ]
})
export class EditorModule { }
