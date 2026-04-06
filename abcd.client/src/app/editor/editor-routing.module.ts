import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { EditorLayoutComponent } from './editor-layout/editor-layout.component';
import { DashboardComponent } from './dashboard/dashboard.component';
import { EditPostComponent } from './edit-post/edit-post.component';
import { AiChatComponent } from './ai-chat/ai-chat.component';
import { SeriesListComponent } from './series-list/series-list.component';
import { EditSeriesComponent } from './edit-series/edit-series.component';

const routes: Routes = [
  {
    path: '',
    component: EditorLayoutComponent,
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'edit/:postId', component: EditPostComponent },
      { path: 'ai-chat', component: AiChatComponent },
      { path: 'series', component: SeriesListComponent },
      { path: 'series/:seriesId', component: EditSeriesComponent }
    ]
  }
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class EditorRoutingModule { }
