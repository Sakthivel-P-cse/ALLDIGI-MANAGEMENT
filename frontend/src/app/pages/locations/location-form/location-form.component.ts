import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { LocationService } from '../../../services/location.service';

@Component({
  selector: 'app-location-form',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './location-form.component.html'
})
export class LocationFormComponent implements OnInit {
  isEdit = false;
  id: number | null = null;
  loading = false;
  error: string | null = null;
  model: any = {
    name: '',
    statusType: 'deployable' // default for status-labels
  };

  constructor(
    private service: LocationService,
    private router: Router,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.isEdit = true;
      this.id = +idParam;
      this.loadItem();
    }
  }

  loadItem(): void {
    if (!this.id) return;
    this.service.getById(this.id).subscribe({
      next: (data) => {
        this.model = { ...data };
      },
      error: () => this.error = 'Failed to load data.'
    });
  }

  onSubmit(): void {
    this.loading = true;
    this.error = null;

    if (this.isEdit && this.id) {
      this.service.update(this.id, this.model).subscribe({
        next: () => this.router.navigate(['/locations']),
        error: (err) => {
          this.error = 'Failed to update.';
          this.loading = false;
          console.error(err);
        }
      });
    } else {
      this.service.create(this.model).subscribe({
        next: () => this.router.navigate(['/locations']),
        error: (err) => {
          this.error = 'Failed to create.';
          this.loading = false;
          console.error(err);
        }
      });
    }
  }
}
