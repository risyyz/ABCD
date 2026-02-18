export interface Fragment {
  fragmentId: number;
  fragmentType: string;
  content: string;
  position: number;
  active?: boolean;
  highlight: boolean;
}
