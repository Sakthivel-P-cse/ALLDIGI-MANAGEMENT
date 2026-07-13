import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AccessoryService } from '../../../services/accessory.service';

@Component({
  selector: 'app-accessory-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './accessory-form.component.html',
  styleUrls: ['../../assets/asset-form/asset-form.component.css']
})
export class AccessoryFormComponent {
  accessoryForm: FormGroup;
  loading = false;
  error: string | null = null;
  pageTitle = 'CREATE NEW ACCESSORY';
  buttonText = 'SAVE ACCESSORY';

  constructor(
    private fb: FormBuilder,
    private accessoryService: AccessoryService,
    private router: Router
  ) {
    this.accessoryForm = this.fb.group({
      name: ['', Validators.required],
      qty: [1, [Validators.required, Validators.min(1)]],
      locationId: ['']
    });
  }

  onSubmit(): void {
    if (this.accessoryForm.invalid) return;

    this.loading = true;
    this.error = null;

    this.accessoryService.createAccessory(this.accessoryForm.value).subscribe({
      next: () => {
        this.loading = false;
        this.router.navigate(['/accessories']);
      },
      error: (err: any) => {
        this.loading = false;
        this.error = 'Failed to create accessory. ' + (err.error?.message || err.message);
        console.error(err);
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/accessories']);
  }
}
