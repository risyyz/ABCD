import { Fragment } from './fragment.model';

export class Post {
  postId: number;
  title: string;
  status: 'Draft' | 'Published';
  dateLastPublished?: string;
  pathSegment?: string;
  synopsis?: string;
  version: string = '';
  fragments: Fragment[] = [];

  constructor(postId: number, title: string, status: 'Draft' | 'Published') {
    this.postId = postId;
    this.title = title;
    this.status = status;
  }

  addFragment(fragment: Fragment) { /* ... */ }
  removeFragment(fragmentId: number) { /* ... */ }
  moveFragment(fromIndex: number, toIndex: number) { /* ... */ }
}
