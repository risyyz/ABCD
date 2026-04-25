import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { Fragment } from '../models/fragment.model';
import { IFragmentComponent } from '../models/fragment-component.interface';
import { EditableBibliography, BibliographyEntry } from '../../models/editable-bibliography.model';

@Component({
  selector: 'app-bibliography-fragment',
  templateUrl: './bibliography-fragment.component.html',
  styleUrls: ['./bibliography-fragment.component.scss'],
  standalone: false
})
export class BibliographyFragmentComponent implements IFragmentComponent, OnInit, OnChanges {
  @Input() fragment!: Fragment;
  editableBibliography: EditableBibliography = new EditableBibliography();
  isEditing: boolean = false;
  private _original: string = '';

  showDialog: boolean = false;
  editingEntry: BibliographyEntry | null = null;
  editingIndex: number = -1;

  ngOnInit() {
    this.initFromFragment();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['fragment']) {
      this.initFromFragment();
    }
  }

  private initFromFragment() {
    if (this.fragment && this.fragment.content) {
      try {
        this.editableBibliography = EditableBibliography.fromJSON(this.fragment.content);
      } catch {
        this.editableBibliography = new EditableBibliography();
      }
    } else {
      this.editableBibliography = new EditableBibliography();
    }
  }

  setEditMode(isEditing: boolean): void {
    this.isEditing = isEditing;
    if (isEditing) {
      this._original = this.editableBibliography.toJSON();
    }
  }

  revert(): void {
    if (this._original) {
      this.editableBibliography = EditableBibliography.fromJSON(this._original);
    }
  }

  getCurrentFragment(): Fragment {
    this.fragment.content = this.editableBibliography.toJSON();
    return this.fragment;
  }

  getData(): string {
    return this.editableBibliography.toJSON();
  }

  edit(): void {
    this.isEditing = true;
  }

  cancel(): void {
    this.isEditing = false;
    this.initFromFragment();
  }

  save(): void {
    this.isEditing = false;
    this.fragment.content = this.getData();
  }

  addEntry(): void {
    this.editingEntry = null;
    this.editingIndex = -1;
    this.showDialog = true;
  }

  editEntry(index: number): void {
    this.editingEntry = this.editableBibliography.entries[index];
    this.editingIndex = index;
    this.showDialog = true;
  }

  onDialogSave(entry: BibliographyEntry): void {
    if (this.editingIndex >= 0) {
      // Update existing entry
      this.editableBibliography.entries[this.editingIndex] = entry;
    } else {
      // Add new entry
      this.editableBibliography.entries.push(entry);
    }
    this.closeDialog();
  }

  formatAuthors(authors: string[]): string {
    if (!authors || authors.length === 0) {
      return 'No authors';
    }
    const filtered = authors.filter((a: string) => a && a.trim());
    return filtered.length > 0 ? filtered.join(', ') : 'No authors';
  }

  closeDialog(): void {
    this.showDialog = false;
    this.editingEntry = null;
    this.editingIndex = -1;
  }

  removeEntry(index: number): void {
    this.editableBibliography.removeEntry(index);
  }

  get maxReferenceNumber(): number {
    if (this.editableBibliography.entries.length === 0) return 0;
    return Math.max(...this.editableBibliography.entries.map(e => e.referenceNumber));
  }

  trackByIndex(index: number): number {
    return index;
  }
}
