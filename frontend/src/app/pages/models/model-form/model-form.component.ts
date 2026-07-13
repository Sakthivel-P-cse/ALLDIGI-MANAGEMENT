import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ModelService } from '../../../services/model.service';

@Component({
  selector: 'app-model-form',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './model-form.component.html'
})
export class ModelFormComponent implements OnInit {
  isEdit = false;
  id: number | null = null;
  loading = false;
  error: string | null = null;
  model: any = {
    name: '',
    statusType: 'deployable' // default for status-labels
  };

  constructor(
    private service: ModelService,
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
        next: () => this.router.navigate(['/models']),
        error: (err) => {
          this.error = 'Failed to update.';
          this.loading = false;
          console.error(err);
        }
      });
    } else {
      this.service.create(this.model).subscribe({
        next: () => this.router.navigate(['/models']),
        error: (err) => {
          this.error = 'Failed to create.';
          this.loading = false;
          console.error(err);
        }
      });
    }
  }
}
