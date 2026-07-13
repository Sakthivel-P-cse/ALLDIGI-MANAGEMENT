import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute, RouterModule } from '@angular/router';
import { AssetService } from '../../../services/asset.service';

import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatDividerModule } from '@angular/material/divider';
import { MatProgressBarModule } from '@angular/material/progress-bar';

@Component({
  selector: 'app-asset-form',
  standalone: true,
  imports: [
    CommonModule, 
    ReactiveFormsModule,
    RouterModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatDividerModule,
    MatProgressBarModule
  ],
  templateUrl: './asset-form.component.html',
  styleUrls: ['./asset-form.component.css']
})
export class AssetFormComponent implements OnInit {
  assetForm: FormGroup;
  loading = false;
  error: string | null = null;
  editId: number | null = null;
  pageTitle = 'CREATE NEW ASSET';
  buttonText = 'SAVE ASSET';

  constructor(
    private fb: FormBuilder,
    private assetService: AssetService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.assetForm = this.fb.group({
      name: ['', Validators.required],
      assetTag: ['', Validators.required],
      modelId: [1, [Validators.required, Validators.min(1)]],
      serial: [''],
      statusId: [''],
      notes: ['']
    });
  }

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.editId = Number(idParam);
      this.pageTitle = `EDIT ASSET`;
      this.buttonText = 'UPDATE ASSET';
      this.loadAssetData(this.editId);
    }
  }

  loadAssetData(id: number): void {
    this.loading = true;
    this.assetService.getAsset(id).subscribe({
      next: (asset) => {
        this.pageTitle = `EDIT ASSET: ${asset.assetTag}`;
        this.assetForm.patchValue({
          name: asset.name,
          assetTag: asset.assetTag,
          modelId: asset.modelId,
          serial: asset.serial,
          statusId: asset.statusId,
          notes: asset.notes
        });
        this.loading = false;
      },
      error: (err: any) => {
        this.error = 'Failed to load asset details for editing.';
        this.loading = false;
        console.error(err);
      }
    });
  }

  onSubmit(): void {
    if (this.assetForm.invalid) return;

    this.loading = true;
    this.error = null;

    const request$ = this.editId 
      ? this.assetService.updateAsset(this.editId, this.assetForm.value)
      : this.assetService.createAsset(this.assetForm.value);

    request$.subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/assets']);
      },
      error: (err: any) => {
        this.loading = false;
        this.error = `Failed to ${this.editId ? 'update' : 'create'} asset. ` + (err.error?.message || err.message);
        console.error(err);
      }
    });
  }

  cancel(): void {
    if (this.editId) {
      this.router.navigate(['/assets', this.editId]);
    } else {
      this.router.navigate(['/assets']);
    }
  }
}
