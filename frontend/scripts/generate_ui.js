const fs = require('fs');
const path = require('path');

const entities = [
  { name: 'Category', plural: 'Categories', pathName: 'categories', service: 'CategoryService', modelFile: 'category' },
  { name: 'Manufacturer', plural: 'Manufacturers', pathName: 'manufacturers', service: 'ManufacturerService', modelFile: 'manufacturer' },
  { name: 'Model', plural: 'Models', pathName: 'models', service: 'ModelService', modelFile: 'model' },
  { name: 'Supplier', plural: 'Suppliers', pathName: 'suppliers', service: 'SupplierService', modelFile: 'supplier' },
  { name: 'StatusLabel', plural: 'StatusLabels', pathName: 'status-labels', service: 'StatusLabelService', modelFile: 'status-label' },
  { name: 'Location', plural: 'Locations', pathName: 'locations', service: 'LocationService', modelFile: 'location' },
  { name: 'Department', plural: 'Departments', pathName: 'departments', service: 'DepartmentService', modelFile: 'department' },
  { name: 'Company', plural: 'Companies', pathName: 'companies', service: 'CompanyService', modelFile: 'company' },
];

function generateListHtml(e) {
  return `<div class="container mt-4">
  <div class="d-flex justify-content-between align-items-center mb-3">
    <h2>${e.plural}</h2>
    <button class="btn btn-primary" routerLink="/${e.pathName}/new" *ngIf="isAdmin">
      <i class="bi bi-plus"></i> Create New
    </button>
  </div>

  <div class="card">
    <div class="card-body">
      <div class="table-responsive">
        <table class="table table-striped table-hover align-middle">
          <thead>
            <tr>
              <th>ID</th>
              <th>Name</th>
              <th>Created At</th>
              <th *ngIf="isAdmin">Actions</th>
            </tr>
          </thead>
          <tbody>
            <tr *ngFor="let item of items">
              <td>{{ item.id }}</td>
              <td>{{ item.name }}</td>
              <td>{{ item.createdAt | date }}</td>
              <td *ngIf="isAdmin">
                <button class="btn btn-sm btn-outline-primary me-2" [routerLink]="['/${e.pathName}', item.id, 'edit']">
                  Edit
                </button>
                <button class="btn btn-sm btn-outline-danger" (click)="deleteItem(item.id)">
                  Delete
                </button>
              </td>
            </tr>
            <tr *ngIf="!loading && items.length === 0">
              <td colspan="4" class="text-center">No ${e.plural.toLowerCase()} found.</td>
            </tr>
            <tr *ngIf="loading">
              <td colspan="4" class="text-center">Loading...</td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</div>`;
}

function generateListTs(e) {
  const compName = `${e.name}ListComponent`;
  const dtoType = `${e.name}ResponseDto`;
  
  return `import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ${e.service} } from '../../../services/${e.modelFile}.service';
import { AuthService } from '../../../services/auth.service';
import { ${dtoType} } from '../../../models/${e.modelFile}.model';

@Component({
  selector: 'app-${e.modelFile}-list',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './${e.modelFile}-list.component.html'
})
export class ${compName} implements OnInit {
  items: ${dtoType}[] = [];
  loading = false;
  total = 0;
  skip = 0;
  take = 50;

  constructor(
    private service: ${e.service},
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
`;
}

function generateFormHtml(e) {
  return `<div class="container mt-4">
  <div class="card mx-auto" style="max-width: 600px;">
    <div class="card-header">
      <h4>{{ isEdit ? 'Edit' : 'Create' }} ${e.name}</h4>
    </div>
    <div class="card-body">
      <div *ngIf="error" class="alert alert-danger">{{ error }}</div>
      <form (ngSubmit)="onSubmit()" #form="ngForm">
        <div class="mb-3">
          <label class="form-label">Name <span class="text-danger">*</span></label>
          <input type="text" class="form-control" name="name" [(ngModel)]="model.name" required #nameField="ngModel">
          <div *ngIf="nameField.invalid && nameField.touched" class="text-danger small">Name is required.</div>
        </div>
        
        <div class="mb-3">
          <label class="form-label">Notes</label>
          <textarea class="form-control" name="notes" [(ngModel)]="model.notes" rows="3"></textarea>
        </div>

        <!-- Add more fields manually as needed -->

        <div class="d-flex justify-content-end mt-4">
          <button type="button" class="btn btn-secondary me-2" routerLink="/${e.pathName}">Cancel</button>
          <button type="submit" class="btn btn-primary" [disabled]="form.invalid || loading">
            <span *ngIf="loading" class="spinner-border spinner-border-sm me-1"></span>
            Save
          </button>
        </div>
      </form>
    </div>
  </div>
</div>`;
}

function generateFormTs(e) {
  const compName = `${e.name}FormComponent`;
  const createDto = `Create${e.name}Dto`;
  const updateDto = `Update${e.name}Dto`;
  
  return `import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router, ActivatedRoute } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { ${e.service} } from '../../../services/${e.modelFile}.service';

@Component({
  selector: 'app-${e.modelFile}-form',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule],
  templateUrl: './${e.modelFile}-form.component.html'
})
export class ${compName} implements OnInit {
  isEdit = false;
  id: number | null = null;
  loading = false;
  error: string | null = null;
  model: any = {
    name: '',
    statusType: 'deployable' // default for status-labels
  };

  constructor(
    private service: ${e.service},
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
        next: () => this.router.navigate(['/${e.pathName}']),
        error: (err) => {
          this.error = 'Failed to update.';
          this.loading = false;
          console.error(err);
        }
      });
    } else {
      this.service.create(this.model).subscribe({
        next: () => this.router.navigate(['/${e.pathName}']),
        error: (err) => {
          this.error = 'Failed to create.';
          this.loading = false;
          console.error(err);
        }
      });
    }
  }
}
`;
}

const basePath = path.join(__dirname, '../src/app/pages');

entities.forEach(e => {
  const listDir = path.join(basePath, e.pathName, `${e.modelFile}-list`);
  const formDir = path.join(basePath, e.pathName, `${e.modelFile}-form`);
  
  if (fs.existsSync(listDir)) {
    fs.writeFileSync(path.join(listDir, `${e.modelFile}-list.component.html`), generateListHtml(e));
    fs.writeFileSync(path.join(listDir, `${e.modelFile}-list.component.ts`), generateListTs(e));
  }
  
  if (fs.existsSync(formDir)) {
    fs.writeFileSync(path.join(formDir, `${e.modelFile}-form.component.html`), generateFormHtml(e));
    fs.writeFileSync(path.join(formDir, `${e.modelFile}-form.component.ts`), generateFormTs(e));
  }
});

console.log('UI generation complete.');
