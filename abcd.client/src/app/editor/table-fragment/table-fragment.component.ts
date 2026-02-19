import { Component, Input, OnInit, OnChanges, SimpleChanges } from '@angular/core';
import { Fragment } from '../models/fragment.model';
import { IFragmentComponent } from '../models/fragment-component.interface';
import { EditableTable, TableCell, TableColumn, CellAlignment } from '../../models/editable-table.model';

@Component({
  selector: 'app-table-fragment',
  templateUrl: './table-fragment.component.html',
  styleUrls: ['./table-fragment.component.scss'],
  standalone: false
})
export class TableFragmentComponent implements IFragmentComponent, OnInit, OnChanges {
  @Input() fragment!: Fragment;
  table: EditableTable = new EditableTable();
  isTableEditing = false;
  private originalTableJson: string = '';

  ngOnInit() {
    this.initTableFromFragment();
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes['fragment']) {
      this.initTableFromFragment();
    }
  }

  private initTableFromFragment() {
    if (this.fragment && this.fragment.content) {
      try {
        this.table = EditableTable.fromJSON(this.fragment.content);
      } catch {
        this.table = new EditableTable();
      }
    } else {
      this.table = new EditableTable();
    }
  }

  setEditMode(isEditing: boolean) {
    this.isTableEditing = isEditing;
    if (isEditing) {
      this.originalTableJson = this.table.toJSON();
    }
  }

  revert() {
    if (this.originalTableJson) {
      this.table = EditableTable.fromJSON(this.originalTableJson);
    }
  }

  getCurrentFragment(): Fragment {
    this.fragment.content = this.table.toJSON();
    return this.fragment;
  }

  addRow() {
    if (this.isTableEditing) this.table.addRow();
  }

  insertRow(index: number) {
    if (this.isTableEditing) this.table.insertRow(index);
  }

  removeRow(index: number) {
    if (this.isTableEditing) this.table.removeRow(index);
  }

  addColumn() {
    if (this.isTableEditing) {
      const colNum = this.table.columns.length + 1;
      this.table.addColumn({ header: `Header ${colNum}` });
    }
  }

  insertColumn(index: number) {
    if (this.isTableEditing) {
      const colNum = index + 1;
      this.table.insertColumn(index, { header: `Header ${colNum}` });
    }
  }

  removeColumn(index: number) {
    if (this.isTableEditing) this.table.removeColumn(index);
  }

  setColumnAlignment(col: number, alignment: CellAlignment) {
    if (this.isTableEditing) this.table.setColumnAlignment(col, alignment);
  }
}
