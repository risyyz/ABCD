import { Component, Input, Output, EventEmitter, ContentChild, AfterContentInit, Type } from '@angular/core';
import { Fragment } from '../models/fragment.model';
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
export class EditableFragmentComponent implements AfterContentInit {
  @ContentChild(CodeFragmentComponent) codeFragmentComp?: IFragmentComponent;
  @ContentChild(RichTextFragmentComponent) richTextFragmentComp?: IFragmentComponent;
  @ContentChild(ImageFragmentComponent) imageFragmentComp?: IFragmentComponent;
  @ContentChild(TableFragmentComponent) tableFragmentComp?: IFragmentComponent;

  @Input() fragmentId!: number;
  @Input() position!: number;
  @Input() fragmentCount!: number;
  @Input() highlight: boolean = false;

  @Output() fragmentEdit = new EventEmitter<void>();
  @Output() fragmentSave = new EventEmitter<Fragment>();
  @Output() fragmentCancel = new EventEmitter<void>();
  @Output() fragmentMoveUp = new EventEmitter<number>();
  @Output() fragmentMoveDown = new EventEmitter<number>();

  isEditing: boolean = false;
  @Input() addFragmentDropdownOpen: boolean = false;
  @Output() addFragmentDropdownOpenChange = new EventEmitter<number | null>();

  constructor() {
    document.addEventListener('click', this.closeFragmentDropdownOnOutsideClick.bind(this));
  }

  ngAfterContentInit() {
    //this.fragmentComponent?.init();
  }

  closeFragmentDropdownOnOutsideClick(event: MouseEvent) {
    if (this.addFragmentDropdownOpen) {
      const path = event.composedPath && event.composedPath();
      if (path && !path.some((el: any) => el.classList && (el.classList.contains('dropdown-trigger') || el.classList.contains('dropdown-menu')))) {
        this.addFragmentDropdownOpen = false;
      }
    }
  }


  onEdit() {
    this.isEditing = true;
    this.fragmentComponent?.setEditMode(true);
    this.fragmentEdit.emit();
  }

  onCancel() {
    this.isEditing = false;
    this.fragmentComponent?.revert();
    this.fragmentCancel.emit();
  }

  onSave() {
    this.isEditing = false;
    this.fragmentComponent?.setEditMode(false);
    this.fragmentSave.emit(this.fragmentComponent?.getCurrentFragment());
  }

  onMoveUp() {
    console.log("moving up " + this.fragmentId);
    this.fragmentMoveUp.emit(this.fragmentId);
  }

  onMoveDown() {
    console.log("moving down " + this.fragmentId);
    this.fragmentMoveDown.emit(this.fragmentId);
  }

  onAddDropdownButtonClick(event: MouseEvent) {
    event.stopPropagation();
    if (!this.addFragmentDropdownOpen) {
      this.addFragmentDropdownOpenChange.emit(this.fragmentId);
    } else {
      this.addFragmentDropdownOpenChange.emit(null);
    }
  }

  onAddFragment(fragment: string) {
    this.addFragmentDropdownOpenChange.emit(null); // close dropdown after selection
  }

  get fragmentComponent(): IFragmentComponent | undefined {
    return (
      this.codeFragmentComp ||
      this.richTextFragmentComp ||
      this.imageFragmentComp ||
      this.tableFragmentComp
    );
  }
}
