import { Component, Input, Output, EventEmitter } from '@angular/core';
import { EditableBaseComponent } from '../editable-base/editable-base.component';

@Component({
  selector: 'app-editable-fragment',
  standalone: false,
  templateUrl: './editable-fragment.component.html',
  styleUrls: ['./editable-fragment.component.scss']
})
export class EditableFragmentComponent extends EditableBaseComponent {
  @Input() position!: number;
  @Input() fragmentCount!: number;
  @Input() highlight: boolean = false;

  @Output() moveUp = new EventEmitter<Number>();
  @Output() moveDown = new EventEmitter<Number>();

  onMoveUp() {
    console.log("moving up" + this.position);
    this.moveUp.emit(this.position);
  }

  onMoveDown() {
    console.log("moving down" + this.position);
    this.moveDown.emit(this.position);
  }
}
