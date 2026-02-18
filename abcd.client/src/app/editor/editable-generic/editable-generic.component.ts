import { Component, Output, EventEmitter } from '@angular/core';

@Component({
  selector: 'app-editable-generic',
  standalone: false,
  templateUrl: './editable-generic.component.html',
  styleUrls: ['./editable-generic.component.scss']
})
export class EditableGenericComponent {
  isEditing: boolean = false;

  @Output() edit = new EventEmitter<void>();
  @Output() save = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();

  onEdit() {
    this.isEditing = true;
    this.edit.emit();
  }

  onCancel() {
    this.isEditing = false;    
    this.cancel.emit();
  }

  onSave() {
    this.isEditing = false;    
    this.save.emit();
  }
}
