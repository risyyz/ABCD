import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { SeriesService, SeriesSummary } from '../../services/series.service';

@Component({
  selector: 'app-series-list',
  standalone: false,
  templateUrl: './series-list.component.html',
  styleUrl: './series-list.component.css'
})
export class SeriesListComponent implements OnInit {
  seriesList: SeriesSummary[] = [];
  showCreateSeries = false;

  constructor(
    private seriesService: SeriesService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.fetchSeries();
  }

  fetchSeries(): void {
    this.seriesService.getAll().subscribe(series => {
      this.seriesList = series;
    });
  }

  openCreateSeries(): void {
    this.showCreateSeries = true;
  }

  closeCreateSeries(): void {
    this.showCreateSeries = false;
    this.fetchSeries();
  }
}
