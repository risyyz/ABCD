import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Post } from '../models/post.model'; 
import { PostService } from '../../services/post.service'; 
import { FormsModule } from '@angular/forms';
import {RichTextFragmentComponent } from '../rich-text-fragment/rich-text-fragment.component'

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

  //save(): void {
  //  if (this.post) {
  //    this.postService.updatePost(this.post).subscribe(() => {
  //      // handle success, e.g., navigate away or show a message
  //    });
  //  }
  //}
}
