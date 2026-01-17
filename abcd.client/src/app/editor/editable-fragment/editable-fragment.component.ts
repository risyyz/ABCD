import { Component, Input, Output, EventEmitter, ContentChild, AfterContentInit, Type } from '@angular/core';
import { Fragment } from '../models/fragment.model';
import { EditableBaseComponent } from '../editable-base/editable-base.component';
import { IFragmentComponent } from '../models/fragment-component.interface';
import { CodeFragmentComponent } from '../code-fragment/code-fragment.component';
import { RichTextFragmentComponent } from '../rich-text-fragment/rich-text-fragment.component';
import { ImageFragmentComponent } from '../image-fragment/image-fragment.component';
import { TableFragmentComponent } from '../table-fragment/table-fragment.component';

@Component({
  selector: 'app-editable-fragment',
  standalone: false,
  templateUrl: './editable-fragment.component.html',
  styleUrls: ['./editable-fragment.component.scss']
})
export class EditableFragmentComponent extends EditableBaseComponent implements AfterContentInit {
  @ContentChild(CodeFragmentComponent) codeFragmentComp?: IFragmentComponent;
  @ContentChild(RichTextFragmentComponent) richTextFragmentComp?: IFragmentComponent;
  @ContentChild(ImageFragmentComponent) imageFragmentComp?: IFragmentComponent;
  @ContentChild(TableFragmentComponent) tableFragmentComp?: IFragmentComponent;

  @Input() position!: number;
  @Input() fragmentCount!: number;
  @Input() highlight: boolean = false;

  @Output() moveUp = new EventEmitter<Number>();
  @Output() moveDown = new EventEmitter<Number>();
  @Output() fragmentSaved = new EventEmitter<Fragment>();


  onMoveUp() {
    console.log("moving up" + this.position);
    this.moveUp.emit(this.position);
  }

  onMoveDown() {
    console.log("moving down" + this.position);
    this.moveDown.emit(this.position);
  }

  getLatestFragment() {
    return (
      this.codeFragmentComp?.getLatestFragment?.() ||
      this.richTextFragmentComp?.getLatestFragment?.() ||
      this.imageFragmentComp?.getLatestFragment?.() ||
      this.tableFragmentComp?.getLatestFragment?.()
    );
  }

  override onSave() {
    super.onSave(); // Call base logic (sets editing state, disables controls, emits save event)
    this.fragmentSaved.emit(this.getLatestFragment());
  }
}
