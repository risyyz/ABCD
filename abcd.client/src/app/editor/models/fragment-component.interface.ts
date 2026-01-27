import { Fragment } from './fragment.model';

export interface IFragmentComponent {
  fragment: Fragment;
  setEditMode(isEditing: boolean): void;
  revert(): void;
  getCurrentFragment(): Fragment;
}
