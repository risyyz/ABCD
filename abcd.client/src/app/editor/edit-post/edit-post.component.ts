import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Post } from '../models/post.model'; 
import { PostService } from '../../services/post.service';
import { Fragment } from '../models/fragment.model';

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

  onFragmentMoveUp(position: Number) {
    if (!this.post) return;

    this.highlightFragment(position);
    const fragments = this.post.fragments;
    const index = fragments.findIndex(f => f.position === position);
    if (index > 0) {
      // Swap with the previous fragment
      [fragments[index - 1].position, fragments[index].position] = [fragments[index].position, fragments[index - 1].position];
      // Re-sort the array by position
      fragments.sort((a, b) => a.position - b.position);
    }
  }

  onFragmentMoveDown(position: Number) {
    if (!this.post) return;

    this.highlightFragment(position);

    const fragments = this.post.fragments;
    const index = fragments.findIndex(f => f.position === position);
    if (index !== -1 && index < fragments.length - 1) {
      // Swap with the next fragment
      [fragments[index + 1].position, fragments[index].position] = [fragments[index].position, fragments[index + 1].position];
      // Re-sort the array by position
      fragments.sort((a, b) => a.position - b.position);
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

  onFragmentSaved(fragment: Fragment) {
    // Handle the saved fragment (e.g., update post, send to server, etc.)
    console.log('Fragment saved:', fragment);
    // Example: update the fragment in post.fragments if needed
  }
}
