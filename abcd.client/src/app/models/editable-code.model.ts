export class EditableCode {
  language: string;
  code: string;
  caption: string;

  constructor(language: string = 'javascript', code: string = '', caption: string = '') {
    this.language = language;
    this.code = code;
    this.caption = caption;
  }

  toJSON(): string {
    return JSON.stringify({ language: this.language, code: this.code, caption: this.caption });
  }

  static fromJSON(json: string): EditableCode {
    const obj = typeof json === 'string' ? JSON.parse(json) : json;
    return new EditableCode(obj.language, obj.code, obj.caption ?? '');
  }
}
