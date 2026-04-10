import { Fragment } from './fragment.model';

export interface ParentPost {
  postId: number;
  title: string;
  pathSegment?: string;
  status: string;
}

export class Post {
  postId: number;
  title: string;
  status: 'Draft' | 'Published';
  dateLastPublished?: string;
  pathSegment?: string;
  synopsis?: string;
  parent?: ParentPost | null;
  version: string = '';
  canPublish: boolean = false;
  publishReasons: string[] = [];
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
