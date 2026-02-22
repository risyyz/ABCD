import { Component, Input, OnChanges, OnDestroy, OnInit, SimpleChanges } from '@angular/core';
import { Fragment } from '../models/fragment.model';
import { IFragmentComponent } from '../models/fragment-component.interface';
import { EditableImage } from '../../models/editable-image.model';
import { PostService } from '../../services/post.service';

@Component({
  selector: 'app-image-fragment',
  templateUrl: './image-fragment.component.html',
  styleUrls: ['./image-fragment.component.scss'],
  standalone: false
})
export class ImageFragmentComponent implements IFragmentComponent, OnInit, OnChanges, OnDestroy {
  @Input() fragment!: Fragment;
  @Input() postId!: number;

  image: EditableImage = new EditableImage();
  previewUrl: string = '';
  selectedFile: File | null = null;
  isEditable: boolean = false;
  isUploading: boolean = false;
  errorMessage: string | null = null;
  uploadSuccessMessage: string | null = null;
  private originalContent: string = '';

  constructor(private postService: PostService) {}

  ngOnInit(): void {
    this.loadFromFragment();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['fragment'] && this.fragment) {
      this.loadFromFragment();
    }
  }

  ngOnDestroy(): void {
    if (this.previewUrl.startsWith('blob:')) {
      URL.revokeObjectURL(this.previewUrl);
    }
  }

  onFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files && input.files[0] ? input.files[0] : null;

    this.errorMessage = null;
    this.uploadSuccessMessage = null;

    if (!file) {
      this.selectedFile = null;
      return;
    }

    this.selectedFile = file;
    if (!this.image.fileName) {
      this.image.fileName = file.name;
    }

    if (this.previewUrl.startsWith('blob:')) {
      URL.revokeObjectURL(this.previewUrl);
    }

    this.previewUrl = URL.createObjectURL(file);
  }

  uploadSelectedFile(): void {
    this.errorMessage = null;
    this.uploadSuccessMessage = null;

    if (!this.postId || this.postId <= 0) {
      this.errorMessage = 'Missing post id for upload.';
      return;
    }

    if (!this.selectedFile) {
      this.errorMessage = 'Select an image file to upload.';
      return;
    }

    if (!this.image.fileName || !this.image.fileName.trim()) {
      this.errorMessage = 'Destination file name is required.';
      return;
    }

    this.isUploading = true;
    this.postService.uploadImage(this.postId, this.selectedFile, this.image.fileName.trim())
      .subscribe({
        next: response => {
          this.image.imageUrl = response.imageUrl;
          this.image.fileName = response.fileName;
          this.previewUrl = response.imageUrl;
          this.uploadSuccessMessage = 'Image uploaded successfully.';
          this.isUploading = false;
          this.selectedFile = null;
        },
        error: _ => {
          this.errorMessage = 'Failed to upload image.';
          this.isUploading = false;
        }
      });
  }

  setEditMode(isEditing: boolean) {
    this.isEditable = isEditing;
    this.errorMessage = null;
    this.uploadSuccessMessage = null;
  }

  revert() {
    this.image = EditableImage.fromJSON(this.originalContent);
    this.previewUrl = this.image.imageUrl;
    this.selectedFile = null;
    this.errorMessage = null;
    this.uploadSuccessMessage = null;
  }

  getCurrentFragment(): Fragment {
    this.fragment.content = this.image.toJSON();
    return this.fragment;
  }

  private loadFromFragment(): void {
    this.image = EditableImage.fromJSON(this.fragment?.content ?? '');
    this.previewUrl = this.image.imageUrl;
    this.originalContent = this.image.toJSON();
    this.selectedFile = null;
    this.errorMessage = null;
    this.uploadSuccessMessage = null;

    console.log(this.originalContent);
    console.log(this.previewUrl);
  }
}
