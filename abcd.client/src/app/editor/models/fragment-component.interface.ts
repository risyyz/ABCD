import { Fragment } from './fragment.model';

export interface IFragmentComponent {
  fragment: Fragment;
  getLatestFragment(): Fragment;
}
