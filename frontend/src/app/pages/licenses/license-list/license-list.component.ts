import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { LicenseService, License } from '../../../services/license.service';
import { AuthService } from '../../../services/auth.service';

@Component({
  selector: 'app-license-list',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './license-list.component.html',
  styleUrls: ['./license-list.component.css']
})
export class LicenseListComponent implements OnInit {
  licenses: License[] = [];
  loading = false;
  error: string | null = null;
  total = 0;
  skip = 0;
  take = 50;

  constructor(
    private licenseService: LicenseService,
    private authService: AuthService
  ) {}

  get isAdmin(): boolean {
    return this.authService.isAdmin();
  }

  ngOnInit(): void {
    this.loadLicenses();
  }

  loadLicenses(): void {
    this.loading = true;
    this.licenseService.listLicenses(this.skip, this.take).subscribe({
      next: (res: any) => {
        this.licenses = res.data || [];
        this.total = res.total || 0;
        this.loading = false;
      },
      error: (err: any) => {
        this.error = 'Failed to load licenses';
        this.loading = false;
        console.error(err);
      }
    });
  }
}
