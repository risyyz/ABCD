import { Component, Input, OnInit } from '@angular/core';
import { EditableBibliography } from '../../models/editable-bibliography.model';

@Component({
  selector: 'app-public-bibliography-fragment',
  templateUrl: './public-bibliography-fragment.component.html',
  styleUrls: ['./public-bibliography-fragment.component.scss'],
  standalone: false
})
export class PublicBibliographyFragmentComponent implements OnInit {
  @Input() content: string = '';
  bibliography: EditableBibliography = new EditableBibliography();

  ngOnInit() {
    if (this.content) {
      try {
        this.bibliography = EditableBibliography.fromJSON(this.content);
      } catch {
        this.bibliography = new EditableBibliography();
      }
    }
  }

  formatAuthors(authors: string[]): string {
    if (!authors || authors.length === 0) return '';

    const validAuthors = authors.filter(a => a && a.trim());
    if (validAuthors.length === 0) return '';

    return validAuthors.join(', ');
  }
}
