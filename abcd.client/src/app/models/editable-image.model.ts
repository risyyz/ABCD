export class EditableImage {
  imageUrl: string;
  caption: string;
  fileName: string;

  constructor(imageUrl: string = '', caption: string = '', fileName: string = '') {
    this.imageUrl = imageUrl;
    this.caption = caption;
    this.fileName = fileName;
  }

  toJSON(): string {
    return JSON.stringify({
      imageUrl: this.imageUrl,
      caption: this.caption,
      fileName: this.fileName
    });
  }

  static fromJSON(json: string | unknown): EditableImage {
    if (!json) {
      return new EditableImage();
    }

    if (typeof json !== 'string') {
      const obj = json as { imageUrl?: string; caption?: string; fileName?: string };
      return new EditableImage(obj.imageUrl ?? '', obj.caption ?? '', obj.fileName ?? '');
    }

    try {
      const obj = JSON.parse(json) as { imageUrl?: string; caption?: string; fileName?: string };
      return new EditableImage(obj.imageUrl ?? '', obj.caption ?? '', obj.fileName ?? '');
    } catch {
      return new EditableImage(json, '', '');
    }
  }
}