import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule, Router } from '@angular/router';
import { AssetService } from '../../../services/asset.service';
import { AuthService } from '../../../services/auth.service';
import { AssetResponseDto } from '../../../models/asset.model';

import { MatCardModule } from '@angular/material/card';
import { MatTabsModule } from '@angular/material/tabs';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatTableModule } from '@angular/material/table';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatChipsModule } from '@angular/material/chips';

@Component({
  selector: 'app-asset-detail',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule,
    MatCardModule,
    MatTabsModule,
    MatButtonModule,
    MatIconModule,
    MatTableModule,
    MatDividerModule,
    MatProgressBarModule,
    MatChipsModule
  ],
  templateUrl: './asset-detail.component.html',
  styleUrls: ['./asset-detail.component.css']
})
export class AssetDetailComponent implements OnInit {
  asset: AssetResponseDto | null = null;
  loading = false;
  error: string | null = null;
  deleting = false;

  // History state
  history: any[] = [];
  historyLoading = false;
  historyError: string | null = null;
  displayedHistoryColumns: string[] = ['actionDate', 'adminName', 'actionType', 'targetName', 'note'];

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private assetService: AssetService,
    private authService: AuthService
  ) {}

  get isAdmin(): boolean {
    return this.authService.isAdmin();
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadAsset(Number(id));
    } else {
      this.error = 'No asset ID provided';
    }
  }

  loadAsset(id: number): void {
    this.loading = true;
    this.assetService.getAsset(id).subscribe({
      next: (asset) => {
        this.asset = asset;
        this.loading = false;
        this.loadHistory(asset.id);
      },
      error: (err: any) => {
        this.error = 'Failed to load asset details';
        this.loading = false;
        console.error(err);
      }
    });
  }

  loadHistory(id: number): void {
    this.historyLoading = true;
    this.assetService.getAssetHistory(id).subscribe({
      next: (res) => {
        this.history = res.data || [];
        this.historyLoading = false;
      },
      error: (err) => {
        this.historyError = 'Failed to load asset history';
        this.historyLoading = false;
        console.error(err);
      }
    });
  }

  checkoutAsset(): void {
    if (!this.asset) return;
    
    // Quick and dirty browser prompt as requested, for rapid integration testing
    const userIdInput = prompt(`Enter User ID to assign ${this.asset.assetTag} to:`);
    if (!userIdInput) return;
    
    const userId = Number(userIdInput);
    if (isNaN(userId)) {
      alert("Invalid User ID");
      return;
    }

    this.loading = true;
    this.assetService.checkoutAsset(this.asset.id, userId).subscribe({
      next: (updatedAsset) => {
        this.asset = updatedAsset;
        this.loading = false;
        this.loadHistory(updatedAsset.id);
      },
      error: (err: any) => {
        this.error = 'Failed to checkout asset. ' + (err.error?.message || err.message);
        this.loading = false;
        console.error(err);
      }
    });
  }

  checkinAsset(): void {
    if (!this.asset || !confirm(`Check in ${this.asset.assetTag}?`)) return;

    this.loading = true;
    this.assetService.checkinAsset(this.asset.id).subscribe({
      next: (updatedAsset) => {
        this.asset = updatedAsset;
        this.loading = false;
        this.loadHistory(updatedAsset.id);
      },
      error: (err: any) => {
        this.error = 'Failed to checkin asset. ' + (err.error?.message || err.message);
        this.loading = false;
        console.error(err);
      }
    });
  }

  deleteAsset(): void {
    if (!this.asset || !confirm(`Are you sure you want to delete ${this.asset.assetTag}?`)) return;
    
    this.deleting = true;
    this.assetService.deleteAsset(this.asset.id).subscribe({
      next: () => {
        this.router.navigate(['/assets']);
      },
      error: (err: any) => {
        this.error = 'Failed to delete asset';
        this.deleting = false;
        console.error(err);
      }
    });
  }
}
