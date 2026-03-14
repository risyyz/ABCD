export class EditableCode {
  language: string;
  code: string;

  constructor(language: string = 'javascript', code: string = '') {
    this.language = language;
    this.code = code;
  }

  toJSON(): string {
    return JSON.stringify({ language: this.language, code: this.code });
  }

  static fromJSON(json: string): EditableCode {
    const obj = typeof json === 'string' ? JSON.parse(json) : json;
    return new EditableCode(obj.language, obj.code);
  }
}
