import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { PublicLayoutComponent } from './layout/public-layout.component';
import { HomeComponent } from './home/home.component';
import { AboutComponent } from './about/about.component';
import { ContactComponent } from './contact/contact.component';
import { PrivacyComponent } from './privacy/privacy.component';

@NgModule({
  declarations: [
    PublicLayoutComponent,
    HomeComponent,
    AboutComponent,
    ContactComponent,
    PrivacyComponent
  ],
  imports: [
    CommonModule,
    RouterModule
  ]
})
export class PublicModule {}

