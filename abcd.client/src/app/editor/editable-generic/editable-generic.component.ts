import { Component, Output, EventEmitter, Input } from '@angular/core';

@Component({
  selector: 'app-editable-generic',
  standalone: false,
  templateUrl: './editable-generic.component.html',
  styleUrls: ['./editable-generic.component.scss']
})
export class EditableGenericComponent {
  @Input() isEditing: boolean = false;

  @Output() edit = new EventEmitter<void>();
  @Output() save = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();

  onEdit() {
    this.edit.emit();
  }

  onCancel() {
    this.cancel.emit();
  }

  onSave() {
    this.save.emit();
  }
}
