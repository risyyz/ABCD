import { Component, Input, Output, EventEmitter } from '@angular/core';
import { EditableBaseComponent } from '../editable-base/editable-base.component';

@Component({
  selector: 'app-editable-generic',
  standalone: false,
  templateUrl: './editable-generic.component.html',
  styleUrls: ['./editable-generic.component.scss']
})
export class EditableGenericComponent extends EditableBaseComponent {
  @Input() position!: number;
  @Input() fragmentCount!: number;
  @Input() highlight: boolean = false;
}
