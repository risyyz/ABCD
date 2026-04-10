import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { Post } from '../models/post.model'; 
import { PostService } from '../../services/post.service';
import { Fragment } from '../models/fragment.model';
import { MoveFragmentRequest } from '../models/move-fragment-request.model';
import { FragmentUpdateRequest } from '../models/fragment-update-request.model';

@Component({
  selector: 'app-edit-post',
  standalone: false,
  templateUrl: './edit-post.component.html'
})

export class EditPostComponent implements OnInit {
  post: Post | null = null;
  errorMessage: string | null = null;
  activeAddFragmentDropdownId: number | null = null;
  isHeaderEditing: boolean = false;
  isAddFirstFragmentOpen: boolean = false;
  selectedParentPostId: number | null = null;
  parentDisplayText: string = '';
  originalHeader: { title: string; synopsis: string; pathSegment?: string; parentPostId: number | null; parentDisplayText: string } | null = null;

  constructor(
    private route: ActivatedRoute,
    private postService: PostService
  ) {}

  ngOnInit(): void {
    const postId = Number(this.route.snapshot.paramMap.get('postId'));

    this.postService.getPost(postId).subscribe(post => {
      this.post = post;
      this.selectedParentPostId = post.parent?.postId ?? null;
      if (post.parent) {
        let text = post.parent.title;
        if (post.parent.pathSegment) {
          text += ` (${post.parent.pathSegment})`;
        }
        text += ` - ${post.parent.status}`;
        this.parentDisplayText = text;
      }
    });
  }

  onFragmentMoveUp(fragmentId: number) {
    if (!this.post) return;
    const fragments = this.post.fragments;
    const index = fragments.findIndex(f => f.fragmentId === fragmentId);
    if (index > 0) {
      const fragment = fragments[index];
      const request: MoveFragmentRequest = {
        postId: this.post.postId,
        fragmentId: fragmentId,
        newPosition: fragment.position - 1,
        version: this.post.version
      };
      this.postService.moveFragment(request)
        .subscribe({
          next: (updatedPost: Post) => {
            this.post = updatedPost;
            this.errorMessage = null;
          },
          error: (err) => {
            this.errorMessage = 'Failed to update fragment position. Please try again.';
          }
        });
    }
  }

  onFragmentMoveDown(fragmentId: number) {
    if (!this.post) return;
    const fragments = this.post.fragments;
    const index = fragments.findIndex(f => f.fragmentId === fragmentId);
    if (index !== -1 && index < fragments.length - 1) {
      const fragment = fragments[index];
      const request: MoveFragmentRequest = {
        postId: this.post.postId,
        fragmentId: fragmentId,
        newPosition: fragment.position + 1,
        version: this.post.version
      };
      this.postService.moveFragment(request)
        .subscribe({
          next: (updatedPost: Post) => {
            this.post = updatedPost;
            this.errorMessage = null;
          },
          error: (err) => {
            this.errorMessage = 'Failed to update fragment position. Please try again.';
          }
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
    if (!this.post) return;
    const request: FragmentUpdateRequest = {
      postId: this.post.postId,
      fragmentId: fragment.fragmentId,
      content: fragment.content,
      version: this.post.version
    };
    this.postService.saveFragment(request)
      .subscribe({
        next: (updatedPost: Post) => {
          this.post = updatedPost;
          this.errorMessage = null;
        },
        error: (err) => {
          this.errorMessage = 'Failed to save fragment. Please try again.';
        }
      });
  }

  onFragmentAdd(event: { afterFragmentId: number, fragmentType: string }) {
    if (!this.post) return;
    this.postService.addFragment(this.post.postId, event.afterFragmentId, event.fragmentType, this.post.version)
      .subscribe({
        next: (updatedPost: Post) => {
          this.post = updatedPost;
          this.errorMessage = null;
        },
        error: (err) => {
          this.errorMessage = 'Failed to add fragment. Please try again.';
        }
      });
  }

  onAddFirstFragment(fragmentType: string) {
    if (!this.post) return;
    this.isAddFirstFragmentOpen = false;
    this.postService.addFragment(this.post.postId, 0, fragmentType, this.post.version)
      .subscribe({
        next: (updatedPost: Post) => {
          this.post = updatedPost;
          this.errorMessage = null;
        },
        error: (err) => {
          this.errorMessage = 'Failed to add fragment. Please try again.';
        }
      });
  }

  onFragmentDelete(fragmentId: number) {
    if (!this.post) return;
    this.postService.deleteFragment(this.post.postId, fragmentId, this.post.version)
      .subscribe({
        next: (updatedPost: Post) => {
          this.post = updatedPost;
          this.errorMessage = null;
        },
        error: (err) => {
          this.errorMessage = 'Failed to delete fragment. Please try again.';
        }
      });
  }

  onHeaderEdit() {
    this.isHeaderEditing = true;
    if (this.post) {
      this.originalHeader = {
        title: this.post.title,
        synopsis: this.post.synopsis ?? '',
        pathSegment: this.post.pathSegment,
        parentPostId: this.selectedParentPostId,
        parentDisplayText: this.parentDisplayText
      };
    }
  }

  onParentPostChange(event: { postId: number | null, displayText: string }) {
    console.log(event.postId + " - " + event.displayText);
    this.selectedParentPostId = event.postId;
    this.parentDisplayText = event.displayText;
  }

  onHeaderCancel() {
    this.isHeaderEditing = false;
    if (this.post && this.originalHeader) {
      this.post.title = this.originalHeader.title;
      this.post.synopsis = this.originalHeader.synopsis;
      this.post.pathSegment = this.originalHeader.pathSegment;
      this.selectedParentPostId = this.originalHeader.parentPostId;
      this.parentDisplayText = this.originalHeader.parentDisplayText;
    }
  }

  onHeaderSave() {
    if (!this.post) return;
    this.postService.savePost(this.post, this.selectedParentPostId)
      .subscribe({
        next: (updatedPost: Post) => {
          this.post = updatedPost;
          this.selectedParentPostId = updatedPost.parent?.postId ?? null;
          this.errorMessage = null;
          this.isHeaderEditing = false;
        },
        error: (err) => {
          if (err?.error && typeof err.error === 'string') {
            this.errorMessage = err.error;
          } else if (err?.error?.message) {
            this.errorMessage = err.error.message;
          } else {
            this.errorMessage = 'Failed to update post.';
          }
          this.isHeaderEditing = true;
        }
      });
  }

  onToggleStatus() {
    if (!this.post) return;
    this.postService.togglePostStatus(this.post.postId, this.post.version)
      .subscribe({
        next: (updatedPost: Post) => {
          this.post = updatedPost;
          this.errorMessage = null;
        },
        error: (err) => {
          if (err?.error && typeof err.error === 'string') {
            this.errorMessage = err.error;
          } else if (err?.error?.message) {
            this.errorMessage = err.error.message;
          } else {
            this.errorMessage = 'Failed to update post status.';
          }
        }
      });
  }
}
