import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { LicenseService } from '../../../services/license.service';

@Component({
  selector: 'app-license-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './license-form.component.html',
  styleUrls: ['../../assets/asset-form/asset-form.component.css']
})
export class LicenseFormComponent {
  licenseForm: FormGroup;
  loading = false;
  error: string | null = null;
  pageTitle = 'CREATE NEW LICENSE';
  buttonText = 'SAVE LICENSE';

  constructor(
    private fb: FormBuilder,
    private licenseService: LicenseService,
    private router: Router
  ) {
    this.licenseForm = this.fb.group({
      name: ['', Validators.required],
      serial: [''],
      seats: [1, [Validators.required, Validators.min(1)]],
      purchaseCost: [0, Validators.min(0)]
    });
  }

  onSubmit(): void {
    if (this.licenseForm.invalid) return;

    this.loading = true;
    this.error = null;

    this.licenseService.createLicense(this.licenseForm.value).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/licenses']);
      },
      error: (err: any) => {
        this.loading = false;
        this.error = 'Failed to create license. ' + (err.error?.message || err.message);
        console.error(err);
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/licenses']);
  }
}
