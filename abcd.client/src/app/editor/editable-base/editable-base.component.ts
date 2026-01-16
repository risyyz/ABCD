import { Input, Output, EventEmitter, ContentChildren, QueryList, ElementRef, AfterContentInit, Directive } from '@angular/core';

@Directive()
export abstract class EditableBaseComponent implements AfterContentInit {
  isEditing = false;

  @Output() save = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();

  @ContentChildren('editableControl', { descendants: true, read: ElementRef }) controls!: QueryList<ElementRef>;
  private originalValues: any[] = [];

  ngAfterContentInit() {
    this.setDisabled(true);
    this.saveOriginalValues();
  }

  setDisabled(disabled: boolean) {
    this.controls.forEach(control => {
      if (control.nativeElement) {
        control.nativeElement.disabled = disabled;
      }
    });
  }

  saveOriginalValues() {
    this.originalValues = this.controls.map(control => control.nativeElement.value);
  }

  restoreOriginalValues() {
    this.controls.forEach((control, i) => {
      control.nativeElement.value = this.originalValues[i];
      control.nativeElement.dispatchEvent(new Event('input'));
    });
  }

  onEdit() {
    this.isEditing = true;
    this.saveOriginalValues();
    this.setDisabled(false);
  }

  onCancel() {
    this.isEditing = false;
    this.restoreOriginalValues();
    this.setDisabled(true);
    this.cancel.emit();
  }

  onSave() {
    this.isEditing = false;
    this.setDisabled(true);
    this.save.emit();
  }
}
