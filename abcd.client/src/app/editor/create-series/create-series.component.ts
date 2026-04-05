import { Component, Output, EventEmitter } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { SeriesService } from '../../services/series.service';

@Component({
  selector: 'app-create-series',
  templateUrl: './create-series.component.html',
  standalone: true,
  imports: [FormsModule]
})
export class CreateSeriesComponent {
  @Output() close = new EventEmitter<void>();
  title: string = '';
  path: string = '';
  description: string = '';
  errorMessage: string = '';

  constructor(private seriesService: SeriesService) {}

  submit() {
    this.errorMessage = '';
    if (this.title && this.path) {
      this.seriesService.create(this.title, this.path, this.description || undefined).subscribe({
        next: () => this.close.emit(),
        error: err => {
          this.errorMessage = err?.error?.error;
        }
      });
    }
  }
}
