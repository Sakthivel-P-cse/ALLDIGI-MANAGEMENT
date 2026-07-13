import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AccessoryService, Accessory } from '../../../services/accessory.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-accessory-list',
  standalone: true,
  imports: [CommonModule, RouterModule, MatCardModule, MatButtonModule, MatIconModule],
  templateUrl: './accessory-list.component.html',
  styleUrls: ['./accessory-list.component.css']
})
export class AccessoryListComponent implements OnInit {
  accessories: Accessory[] = [];
  loading = false;
  error: string | null = null;
  total = 0;
  skip = 0;
  take = 50;

  constructor(
    private accessoryService: AccessoryService,
    private authService: AuthService
  ) {}

  get isAdmin(): boolean {
    return this.authService.isAdmin();
  }

  ngOnInit(): void {
    this.loadAccessories();
  }

  loadAccessories(): void {
    this.loading = true;
    this.accessoryService.listAccessories(this.skip, this.take).subscribe({
      next: (res: any) => {
        this.accessories = res.data || [];
        this.total = res.total || 0;
        this.loading = false;
      },
      error: (err: any) => {
        this.error = 'Failed to load accessories';
        this.loading = false;
        console.error(err);
      }
    });
  }
}
