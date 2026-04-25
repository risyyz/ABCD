import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { BibliographyEntry } from '../../models/editable-bibliography.model';

@Component({
  selector: 'app-bibliography-entry-dialog',
  templateUrl: './bibliography-entry-dialog.component.html',
  styleUrls: ['./bibliography-entry-dialog.component.scss'],
  standalone: false
})
export class BibliographyEntryDialogComponent implements OnInit {
  @Input() entry: BibliographyEntry | null = null;
  @Input() maxReferenceNumber: number = 0;
  @Output() save = new EventEmitter<BibliographyEntry>();
  @Output() close = new EventEmitter<void>();

  editingEntry: BibliographyEntry = new BibliographyEntry();

  ngOnInit() {
    if (this.entry) {
      // Edit mode - copy the entry
      this.editingEntry = new BibliographyEntry(
        this.entry.referenceNumber,
        [...this.entry.authors],
        this.entry.year,
        this.entry.title,
        this.entry.publisherOrJournal
      );
    } else {
      // Add mode - create new entry with next reference number
      this.editingEntry = new BibliographyEntry(this.maxReferenceNumber + 1);
    }
  }

  addAuthor(): void {
    this.editingEntry.authors.push('');
  }

  removeAuthor(index: number): void {
    if (this.editingEntry.authors.length > 1) {
      this.editingEntry.authors.splice(index, 1);
    }
  }

  onSave(): void {
    // Filter out empty authors
    this.editingEntry.authors = this.editingEntry.authors.filter((a: string) => a && a.trim());
    
    // Ensure at least one author
    if (this.editingEntry.authors.length === 0) {
      this.editingEntry.authors = [''];
    }
    
    this.save.emit(this.editingEntry);
  }

  onClose(): void {
    this.close.emit();
  }

  trackByIndex(index: number): number {
    return index;
  }

  get isValid(): boolean {
    return this.editingEntry.referenceNumber > 0 &&
           this.editingEntry.authors.some((a: string) => a && a.trim()) &&
           this.editingEntry.year > 0 &&
           this.editingEntry.title.trim().length > 0 &&
           this.editingEntry.publisherOrJournal.trim().length > 0;
  }
}
