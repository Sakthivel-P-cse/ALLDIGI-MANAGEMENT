import { Component, OnInit } from '@angular/core';
import { CommonModule, JsonPipe } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormGroup, FormControl, ReactiveFormsModule } from '@angular/forms';
import { DashboardService, DashboardSummary } from '../../services/dashboard.service';

import { MatCardModule } from '@angular/material/card';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatListModule } from '@angular/material/list';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { MatFormFieldModule } from '@angular/material/form-field';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule,
    ReactiveFormsModule,
    JsonPipe,
    MatCardModule,
    MatIconModule,
    MatButtonModule,
    MatProgressBarModule,
    MatListModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatFormFieldModule
  ],
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.css']
})
export class DashboardComponent implements OnInit {
  summary: DashboardSummary | null = null;
  loading = false;
  error: string | null = null;
  
  dateRange = new FormGroup({
    start: new FormControl<Date | null>(null),
    end: new FormControl<Date | null>(null),
  });
  
  // Dummy data for charts/mockups
  upcomingExpirations = [
    { name: 'Microsoft 365 E3', licenses: 5, date: 'Jun 25, 2026' },
    { name: 'Adobe Creative Cloud', licenses: 2, date: 'Jun 30, 2026' },
    { name: 'Autodesk AutoCAD', licenses: 1, date: 'Jul 15, 2026' }
  ];

  constructor(private dashboardService: DashboardService) {
    // Set a default date range: last 30 days
    const end = new Date();
    const start = new Date();
    start.setDate(end.getDate() - 30);
    this.dateRange.setValue({ start, end });
  }

  ngOnInit(): void {
    this.loading = true;
    this.dashboardService.getSummary().subscribe({
      next: (data) => {
        this.summary = data;
        this.loading = false;
      },
      error: (err) => {
        this.error = 'Failed to load dashboard data. ' + (err.error?.message || err.message);
        this.loading = false;
        console.error(err);
      }
    });
    
    // Optional: listen to date changes to reload data
    // this.dateRange.valueChanges.subscribe(val => { ... });
  }
}

