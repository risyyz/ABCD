import { Component } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-public-layout',
  templateUrl: './public-layout.component.html',
  styleUrls: ['./public-layout.component.scss'],
  standalone: false
})
export class PublicLayoutComponent {
  searchQuery = '';
  buildNumber = environment.buildNumber;
  lastPublished = environment.lastPublished;

  constructor(private router: Router) {}

  onSearch(): void {
    const q = this.searchQuery.trim();
    this.router.navigate(['/'], { queryParams: q ? { q } : {} });
  }
}
