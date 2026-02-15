import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Post } from '../models/post.model'; 
import { PostService } from '../../services/post.service';
import { Fragment } from '../models/fragment.model';
import { FragmentPositionChangeRequest } from '../models/fragment-position-change-request.model';

@Component({
  selector: 'app-edit-post',
  standalone: false,
  templateUrl: './edit-post.component.html'
})

export class EditPostComponent implements OnInit {
  post: Post | null = null;

  constructor(
    private route: ActivatedRoute,
    private postService: PostService
  ) {}

  ngOnInit(): void {
    const postId = Number(this.route.snapshot.paramMap.get('postId'));
    this.postService.getPost(postId).subscribe(post => {
      this.post = post;
    });
  }

  onFragmentMoveUp(fragmentId: number) {
    if (!this.post) return;
    const fragments = this.post.fragments;
    const index = fragments.findIndex(f => f.fragmentId === fragmentId);
    if (index > 0) {
      const fragment = fragments[index];
      const request: FragmentPositionChangeRequest = {
        postId: this.post.postId,
        fragmentId: fragmentId,
        newPosition: fragment.position - 1,
        version: this.post.version
      };
      this.postService.updateFragmentPosition(request)
        .subscribe((updatedPost: Post) => {
          this.post = updatedPost;
        });
    }
  }

  onFragmentMoveDown(fragmentId: number) {
    if (!this.post) return;
    const fragments = this.post.fragments;
    const index = fragments.findIndex(f => f.fragmentId === fragmentId);
    if (index !== -1 && index < fragments.length - 1) {
      const fragment = fragments[index];
      const request: FragmentPositionChangeRequest = {
        postId: this.post.postId,
        fragmentId: fragmentId,
        newPosition: fragment.position + 1,
        version: this.post.version
      };
      this.postService.updateFragmentPosition(request)
        .subscribe((updatedPost: Post) => {
          this.post = updatedPost;
        });
    }
  }

  highlightFragment(position: Number) {
    if (!this.post) return;

    console.log("highlighting" + position);
    const fragment = this.post.fragments.find(f => f.position === position);
    if (fragment) {
      fragment.highlight = true;
      setTimeout(() => fragment.highlight = false, 5000);
    }
  }

  onFragmentSave(fragment: Fragment) {
    // Handle the saved fragment (e.g., update post, send to server, etc.)
    console.log('Fragment saved:', fragment);
    // Example: update the fragment in post.fragments if needed
  }
}
