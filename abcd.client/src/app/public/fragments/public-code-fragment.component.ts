import { Component, Input, OnInit } from '@angular/core';
import { PublicFragment } from '../../services/post.service';
import { EditableCode } from '../../models/editable-code.model';
import hljs from 'highlight.js/lib/core';
import javascript from 'highlight.js/lib/languages/javascript';
import typescript from 'highlight.js/lib/languages/typescript';
import csharp from 'highlight.js/lib/languages/csharp';
import xml from 'highlight.js/lib/languages/xml';
import python from 'highlight.js/lib/languages/python';
import sql from 'highlight.js/lib/languages/sql';

hljs.registerLanguage('javascript', javascript);
hljs.registerLanguage('typescript', typescript);
hljs.registerLanguage('csharp', csharp);
hljs.registerLanguage('html', xml);
hljs.registerLanguage('python', python);
hljs.registerLanguage('sql', sql);

@Component({
  selector: 'app-public-code-fragment',
  templateUrl: './public-code-fragment.component.html',
  styleUrls: ['./public-code-fragment.component.scss'],
  standalone: false
})
export class PublicCodeFragmentComponent implements OnInit {
  @Input() fragment!: PublicFragment;
  code: EditableCode = new EditableCode();
  highlightedCode: string = '';

  ngOnInit(): void {
    if (this.fragment?.content) {
      try {
        this.code = EditableCode.fromJSON(this.fragment.content);
        const result = hljs.highlight(this.code.code, { language: this.code.language });
        this.highlightedCode = result.value;
      } catch {
        this.code = new EditableCode();
        this.highlightedCode = '';
      }
    }
  }
}

