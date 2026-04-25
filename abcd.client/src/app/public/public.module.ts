import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { PublicLayoutComponent } from './layout/public-layout.component';
import { HomeComponent } from './home/home.component';
import { AboutComponent } from './about/about.component';
import { ContactComponent } from './contact/contact.component';
import { PrivacyComponent } from './privacy/privacy.component';
import { PostDetailComponent } from './post-detail/post-detail.component';
import { PublicCodeFragmentComponent } from './fragments/public-code-fragment.component';
import { PublicRichTextFragmentComponent } from './fragments/public-richtext-fragment.component';
import { PublicImageFragmentComponent } from './fragments/public-image-fragment.component';
import { PublicTableFragmentComponent } from './fragments/public-table-fragment.component';
import { PublicBibliographyFragmentComponent } from './fragments/public-bibliography-fragment.component';

@NgModule({
  declarations: [
    PublicLayoutComponent,
    HomeComponent,
    AboutComponent,
    ContactComponent,
    PrivacyComponent,
    PostDetailComponent,
    PublicCodeFragmentComponent,
    PublicRichTextFragmentComponent,
    PublicImageFragmentComponent,
    PublicTableFragmentComponent,
    PublicBibliographyFragmentComponent
  ],
  imports: [
    CommonModule,
    RouterModule,
    FormsModule
  ]
})
export class PublicModule {}

