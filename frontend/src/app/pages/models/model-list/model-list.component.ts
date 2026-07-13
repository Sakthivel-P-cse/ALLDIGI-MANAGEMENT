import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ModelService } from '../../../services/model.service';
import { AuthService } from '../../../services/auth.service';
import { ModelResponseDto } from '../../../models/model.model';

@Component({
  selector: 'app-model-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './model-list.component.html'
})
export class ModelListComponent implements OnInit {
  items: ModelResponseDto[] = [];
  loading = false;
  total = 0;
  skip = 0;
  take = 50;

  constructor(
    private service: ModelService,
    private authService: AuthService
  ) {}

  get isAdmin(): boolean { return this.authService.isAdmin(); }

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.service.list(this.skip, this.take).subscribe({
      next: (res: any) => {
        this.items = res.data || [];
        this.total = res.total || 0;
        this.loading = false;
      },
      error: () => this.loading = false
    });
  }

  deleteItem(id: number): void {
    if (confirm('Are you sure you want to delete this item?')) {
      this.service.delete(id).subscribe(() => this.load());
    }
  }
}
