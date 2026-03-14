export type CellAlignment = 'left' | 'center' | 'right';

export interface TableColumn {
  header: string;
  maxWidth?: number; // in pixels
  alignment?: CellAlignment;
}

export interface TableCell {
  value: string;
}

export class EditableTable {
  caption: string;
  columns: TableColumn[];
  rows: TableCell[][];

  constructor(columnDefs?: TableColumn[], rowCount?: number, caption?: string) {
    this.caption = caption ?? '';
    this.columns = columnDefs ?? [];
    this.rows = [];
    if (columnDefs && rowCount && rowCount > 0) {
      this.rows = Array.from({ length: rowCount }, () =>
        columnDefs.map(() => ({ value: '' }))
      );
    }
  }

  addRow() {
    this.rows.push(this.columns.map(() => ({ value: '' })));
  }

  insertRow(index: number) {
    this.rows.splice(index, 0, this.columns.map(() => ({ value: '' })));
  }

  removeRow(index: number) {
    this.rows.splice(index, 1);
  }

  addColumn(column: TableColumn) {
    this.columns.push(column);
    this.rows.forEach(row => row.push({ value: '' }));
  }

  insertColumn(index: number, column?: TableColumn) {
    const col: TableColumn = column ?? { header: `Header ${index + 1}`, alignment: 'left' };
    this.columns.splice(index, 0, col);
    this.rows.forEach(row => row.splice(index, 0, { value: '' }));
  }

  removeColumn(colIndex: number) {
    this.columns.splice(colIndex, 1);
    this.rows.forEach(row => row.splice(colIndex, 1));
  }

  setColumnAlignment(col: number, alignment: CellAlignment) {
    if (this.columns[col]) {
      this.columns[col].alignment = alignment;
    }
  }

  toJSON(): string {
    return JSON.stringify({ caption: this.caption, columns: this.columns, rows: this.rows });
  }

  static fromJSON(json: string): EditableTable {
    const obj = JSON.parse(json);
    const table = new EditableTable(obj.columns, 0, obj.caption);
    table.rows = obj.rows;
    return table;
  }
}
