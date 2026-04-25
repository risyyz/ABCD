export class BibliographyEntry {
  referenceNumber: number;
  authors: string[];
  year: number;
  title: string;
  publisherOrJournal: string;

  constructor(
    referenceNumber: number = 1,
    authors: string[] = [''],
    year: number = new Date().getFullYear(),
    title: string = '',
    publisherOrJournal: string = ''
  ) {
    this.referenceNumber = referenceNumber;
    this.authors = authors;
    this.year = year;
    this.title = title;
    this.publisherOrJournal = publisherOrJournal;
  }
}

export class EditableBibliography {
  entries: BibliographyEntry[];

  constructor(entries: BibliographyEntry[] = []) {
    this.entries = entries.length > 0 ? entries : [new BibliographyEntry()];
  }

  toJSON(): string {
    return JSON.stringify({ entries: this.entries });
  }

  static fromJSON(json: string): EditableBibliography {
    try {
      const obj = typeof json === 'string' ? JSON.parse(json) : json;
      const entries = (obj.entries || []).map((entry: any) => 
        new BibliographyEntry(
          entry.referenceNumber,
          entry.authors || [''],
          entry.year,
          entry.title,
          entry.publisherOrJournal
        )
      );
      return new EditableBibliography(entries);
    } catch {
      return new EditableBibliography();
    }
  }

  addEntry(): void {
    const nextRefNumber = this.entries.length > 0 
      ? Math.max(...this.entries.map(e => e.referenceNumber)) + 1 
      : 1;
    this.entries.push(new BibliographyEntry(nextRefNumber));
  }

  removeEntry(index: number): void {
    if (this.entries.length > 1) {
      this.entries.splice(index, 1);
    }
  }

  addAuthor(entryIndex: number): void {
    if (this.entries[entryIndex]) {
      this.entries[entryIndex].authors.push('');
    }
  }

  removeAuthor(entryIndex: number, authorIndex: number): void {
    if (this.entries[entryIndex] && this.entries[entryIndex].authors.length > 1) {
      this.entries[entryIndex].authors.splice(authorIndex, 1);
    }
  }
}
