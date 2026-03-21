import { Routes, UrlMatcher, UrlSegment, UrlMatchResult } from '@angular/router';
import { PublicLayoutComponent } from './layout/public-layout.component';
import { HomeComponent } from './home/home.component';
import { AboutComponent } from './about/about.component';
import { ContactComponent } from './contact/contact.component';
import { PrivacyComponent } from './privacy/privacy.component';
import { PostDetailComponent } from './post-detail/post-detail.component';

const RESERVED_PATHS = new Set(['editor', 'auth']);

export const postSegmentMatcher: UrlMatcher = (segments: UrlSegment[]): UrlMatchResult | null => {
  if (segments.length === 1 && !RESERVED_PATHS.has(segments[0].path)) {
    return { consumed: segments, posParams: { pathSegment: segments[0] } };
  }
  return null;
};

export const PUBLIC_ROUTES: Routes = [
  {
    path: '',
    component: PublicLayoutComponent,
    children: [
      { path: '', component: HomeComponent, title: 'Home' },
      { path: 'about', component: AboutComponent, title: 'About' },
      { path: 'contact', component: ContactComponent, title: 'Contact' },
      { path: 'privacy', component: PrivacyComponent, title: 'Privacy Policy' },
      { matcher: postSegmentMatcher, component: PostDetailComponent }
    ]
  }
];


