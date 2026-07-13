import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { AssetService } from '../../../services/asset.service';
import { AuthService } from '../../../services/auth.service';
import { AssetResponseDto } from '../../../models/asset.model';
import { ConfirmService } from '../../../services/confirm.service';
import { NotificationService } from '../../../services/notification.service';

import { MatTableModule, MatTableDataSource } from '@angular/material/table';
import { MatPaginatorModule, MatPaginator } from '@angular/material/paginator';
import { MatSortModule, MatSort } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatMenuModule } from '@angular/material/menu';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatCardModule } from '@angular/material/card';
import { MatChipsModule } from '@angular/material/chips';

@Component({
  selector: 'app-asset-list',
  standalone: true,
  imports: [
    CommonModule, 
    RouterModule, 
    FormsModule,
    MatTableModule,
    MatPaginatorModule,
    MatSortModule,
    MatButtonModule,
    MatIconModule,
    MatInputModule,
    MatFormFieldModule,
    MatMenuModule,
    MatProgressBarModule,
    MatCardModule,
    MatChipsModule
  ],
  templateUrl: './asset-list.component.html',
  styleUrls: ['./asset-list.component.css']
})
export class AssetListComponent implements OnInit {
  displayedColumns: string[] = ['assetTag', 'name', 'modelId', 'serial', 'createdAt', 'assignedToName', 'actions'];
  dataSource = new MatTableDataSource<AssetResponseDto>([]);
  
  @ViewChild(MatPaginator) paginator!: MatPaginator;
  @ViewChild(MatSort) sort!: MatSort;
  
  loading = false;
  error: string | null = null;
  total = 0;
  skip = 0;
  take = 50;
  searchQuery = '';
  searchLoading = false;

  constructor(
    private assetService: AssetService,
    private authService: AuthService,
    private router: Router,
    private confirm: ConfirmService,
    private notify: NotificationService
  ) {}

  get isAdmin(): boolean {
    return this.authService.isAdmin();
  }

  ngOnInit(): void {
    if (!this.isAdmin) {
      this.displayedColumns = ['assetTag', 'name', 'modelId', 'serial', 'createdAt', 'actions'];
    }
    this.loadAssets();
  }

  loadAssets(): void {
    this.loading = true;
    this.assetService.listAssets(this.skip, this.take).subscribe({
      next: (res: any) => {
        this.dataSource.data = res.data || [];
        this.total = res.total || 0;
        this.loading = false;
        
        setTimeout(() => {
          this.dataSource.paginator = this.paginator;
          this.dataSource.sort = this.sort;
        });
      },
      error: (err: any) => {
        this.error = 'Failed to load assets';
        this.loading = false;
        console.error(err);
      }
    });
  }

  applyFilter(event: Event) {
    const filterValue = (event.target as HTMLInputElement).value;
    this.dataSource.filter = filterValue.trim().toLowerCase();

    if (this.dataSource.paginator) {
      this.dataSource.paginator.firstPage();
    }
  }

  searchByTag(): void {
    if (!this.searchQuery.trim()) {
      this.loadAssets();
      return;
    }
    
    this.searchLoading = true;
    this.assetService.getByTag(this.searchQuery.trim()).subscribe({
      next: (asset) => {
        this.searchLoading = false;
        if (asset && asset.id) {
          this.router.navigate(['/assets', asset.id]);
        }
      },
      error: (err: any) => {
        this.searchLoading = false;
        this.error = 'Asset not found with tag: ' + this.searchQuery;
      }
    });
  }

  exportCsv(): void {
    this.assetService.exportCsv().subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `assets_export_${new Date().getTime()}.csv`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      },
      error: (err: any) => {
        this.error = 'Failed to export CSV';
        console.error(err);
      }
    });
  }

  async deleteAsset(id: number): Promise<void> {
    const ok = await this.confirm.confirmDelete('this asset');
    if (!ok) return;

    this.assetService.deleteAsset(id).subscribe({
      next: () => {
        this.notify.success('Asset deleted successfully');
        this.loadAssets();
      },
      error: (err: any) => {
        this.notify.error(err.error?.message || 'Failed to delete asset');
      }
    });
  }
}
