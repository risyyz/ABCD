import { Component, EventEmitter, HostBinding, Input, Output, OnChanges, OnInit, OnDestroy, SimpleChanges, HostListener, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subject, Subscription, of } from 'rxjs';
import { debounceTime, distinctUntilChanged, switchMap } from 'rxjs/operators';
import { EditorPostSummary, PostService } from '../../services/post.service';

@Component({
  selector: 'app-post-autocomplete',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './post-autocomplete.component.html',
  styleUrls: ['./post-autocomplete.component.css']
})
export class PostAutocompleteComponent implements OnChanges, OnInit, OnDestroy {
  @Input() selectedPostId: number | null = null;
  @Input() excludePostId: number | null = null;
  @Input() initialDisplayText: string = '';
  @Input() disabled = false;
  @Output() selectedPostIdChange = new EventEmitter<{ postId: number | null, displayText: string }>();

  @HostBinding('class.is-empty') get isEmpty() { return !this.searchText && this.selectedPostId == null; }
  @HostBinding('class.is-focused') isFocused = false;

  searchText = '';
  isOpen = false;
  filteredOptions: EditorPostSummary[] = [];
  highlightedIndex = -1;
  isLoading = false;

  private searchSubject = new Subject<string>();
  private searchSubscription: Subscription | null = null;

  static readonly MIN_SEARCH_LENGTH = 3;

  constructor(private elRef: ElementRef, private postService: PostService) {}

  ngOnInit(): void {
    this.searchSubscription = this.searchSubject.pipe(
      debounceTime(300),
      distinctUntilChanged(),
      switchMap(term => {
        if (term.length < PostAutocompleteComponent.MIN_SEARCH_LENGTH) {
          this.isLoading = false;
          return of([]);
        }
        this.isLoading = true;
        return this.postService.searchPosts(term, this.excludePostId ?? undefined);
      })
    ).subscribe(results => {
      this.filteredOptions = results;
      this.isLoading = false;
      this.highlightedIndex = -1;
    });
  }

  ngOnDestroy(): void {
    this.searchSubscription?.unsubscribe();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['selectedPostId']) {
      if (this.selectedPostId == null) {
        this.searchText = '';
        this.filteredOptions = [];
      } else if (this.initialDisplayText) {
        this.searchText = this.initialDisplayText;
      }
    }
    if (changes['initialDisplayText'] && this.initialDisplayText) {
      this.searchText = this.initialDisplayText;
    }
  }

  onFocus(): void {
    if (this.disabled) return;
    this.isFocused = true;
    if (this.searchText.length >= PostAutocompleteComponent.MIN_SEARCH_LENGTH) {
      this.isOpen = true;
    }
  }

  onBlur(): void {
    this.isFocused = false;
  }

  onInput(): void {
    this.isOpen = true;
    this.selectedPostId = null;
    this.selectedPostIdChange.emit({ postId: null, displayText: '' });
    this.searchSubject.next(this.searchText.trim());
  }

  onKeyDown(event: KeyboardEvent): void {
    if (!this.isOpen || this.filteredOptions.length === 0) return;

    switch (event.key) {
      case 'ArrowDown':
        event.preventDefault();
        this.highlightedIndex = Math.min(this.highlightedIndex + 1, this.filteredOptions.length - 1);
        break;
      case 'ArrowUp':
        event.preventDefault();
        this.highlightedIndex = Math.max(this.highlightedIndex - 1, 0);
        break;
      case 'Enter':
        event.preventDefault();
        if (this.highlightedIndex >= 0) {
          this.selectOption(this.filteredOptions[this.highlightedIndex]);
        }
        break;
      case 'Escape':
        this.close();
        break;
    }
  }

  selectOption(option: EditorPostSummary): void {
    this.selectedPostId = option.postId;
    this.searchText = this.formatOption(option);
    this.selectedPostIdChange.emit({ postId: option.postId, displayText: this.searchText });
    this.close();
  }

  clearSelection(): void {
    this.selectedPostId = null;
    this.searchText = '';
    this.filteredOptions = [];
    this.selectedPostIdChange.emit({ postId: null, displayText: '' });
  }

  formatOption(option: EditorPostSummary): string {
    let text = option.title;
    if (option.pathSegment) {
      text += ` (${option.pathSegment})`;
    }
    return `${text} - ${option.status}`;
  }

  close(): void {
    this.isOpen = false;
    this.highlightedIndex = -1;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: Event): void {
    if (!this.elRef.nativeElement.contains(event.target)) {
      this.close();
    }
  }
}
