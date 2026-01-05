export interface Post {
  postId: number;
  title: string;
  status: 'Draft' | 'Published';
  dateLastPublished?: string;
  pathSegment?: string;
  synopsis?: string;
}
