import { Routes } from '@angular/router';
import { LayoutComponent } from './components/layout/layout.component';
import { AuthGuard } from './auth.guard';
import { AdminGuard } from './admin.guard';

export const routes: Routes = [
  {
    path: 'login',
    loadComponent: () =>
      import('./pages/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: '',
    component: LayoutComponent,
    canActivate: [AuthGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      {
        path: 'dashboard',
        loadComponent: () =>
          import('./pages/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },

      // Assets
      {
        path: 'assets',
        loadComponent: () =>
          import('./pages/assets/asset-list/asset-list.component').then(m => m.AssetListComponent)
      },
      {
        path: 'assets/new',
        canActivate: [AdminGuard],
        loadComponent: () =>
          import('./pages/assets/asset-form/asset-form.component').then(m => m.AssetFormComponent)
      },
      {
        path: 'assets/:id/edit',
        canActivate: [AdminGuard],
        loadComponent: () =>
          import('./pages/assets/asset-form/asset-form.component').then(m => m.AssetFormComponent)
      },
      {
        path: 'assets/:id',
        loadComponent: () =>
          import('./pages/assets/asset-detail/asset-detail.component').then(m => m.AssetDetailComponent)
      },

      // Users
      {
        path: 'users',
        canActivate: [AdminGuard],
        loadComponent: () =>
          import('./pages/users/user-list/user-list.component').then(m => m.UserListComponent)
      },
      {
        path: 'users/new',
        canActivate: [AdminGuard],
        loadComponent: () =>
          import('./pages/users/user-form/user-form.component').then(m => m.UserFormComponent)
      },
      {
        path: 'users/:id/edit',
        canActivate: [AdminGuard],
        loadComponent: () =>
          import('./pages/users/user-form/user-form.component').then(m => m.UserFormComponent)
      },
      {
        path: 'users/:id',
        canActivate: [AdminGuard],
        loadComponent: () =>
          import('./pages/users/user-detail/user-detail.component').then(m => m.UserDetailComponent)
      },

      // Licenses
      {
        path: 'licenses',
        loadComponent: () =>
          import('./pages/licenses/license-list/license-list.component').then(m => m.LicenseListComponent)
      },
      {
        path: 'licenses/new',
        canActivate: [AdminGuard],
        loadComponent: () =>
          import('./pages/licenses/license-form/license-form.component').then(m => m.LicenseFormComponent)
      },

      // Accessories
      {
        path: 'accessories',
        loadComponent: () =>
          import('./pages/accessories/accessory-list/accessory-list.component').then(m => m.AccessoryListComponent)
      },
      {
        path: 'accessories/new',
        canActivate: [AdminGuard],
        loadComponent: () =>
          import('./pages/accessories/accessory-form/accessory-form.component').then(m => m.AccessoryFormComponent)
      },
      // Reference Data
      {
        path: 'categories',
        loadComponent: () => import('./pages/categories/category-list/category-list.component').then(m => m.CategoryListComponent)
      },
      {
        path: 'categories/new',
        canActivate: [AdminGuard],
        loadComponent: () => import('./pages/categories/category-form/category-form.component').then(m => m.CategoryFormComponent)
      },
      {
        path: 'categories/:id/edit',
        canActivate: [AdminGuard],
        loadComponent: () => import('./pages/categories/category-form/category-form.component').then(m => m.CategoryFormComponent)
      },
      {
        path: 'manufacturers',
        loadComponent: () => import('./pages/manufacturers/manufacturer-list/manufacturer-list.component').then(m => m.ManufacturerListComponent)
      },
      {
        path: 'manufacturers/new',
        canActivate: [AdminGuard],
        loadComponent: () => import('./pages/manufacturers/manufacturer-form/manufacturer-form.component').then(m => m.ManufacturerFormComponent)
      },
      {
        path: 'manufacturers/:id/edit',
        canActivate: [AdminGuard],
        loadComponent: () => import('./pages/manufacturers/manufacturer-form/manufacturer-form.component').then(m => m.ManufacturerFormComponent)
      },
      {
        path: 'models',
        loadComponent: () => import('./pages/models/model-list/model-list.component').then(m => m.ModelListComponent)
      },
      {
        path: 'models/new',
        canActivate: [AdminGuard],
        loadComponent: () => import('./pages/models/model-form/model-form.component').then(m => m.ModelFormComponent)
      },
      {
        path: 'models/:id/edit',
        canActivate: [AdminGuard],
        loadComponent: () => import('./pages/models/model-form/model-form.component').then(m => m.ModelFormComponent)
      },
      {
        path: 'suppliers',
        loadComponent: () => import('./pages/suppliers/supplier-list/supplier-list.component').then(m => m.SupplierListComponent)
      },
      {
        path: 'suppliers/new',
        canActivate: [AdminGuard],
        loadComponent: () => import('./pages/suppliers/supplier-form/supplier-form.component').then(m => m.SupplierFormComponent)
      },
      {
        path: 'suppliers/:id/edit',
        canActivate: [AdminGuard],
        loadComponent: () => import('./pages/suppliers/supplier-form/supplier-form.component').then(m => m.SupplierFormComponent)
      },
      {
        path: 'status-labels',
        loadComponent: () => import('./pages/status-labels/status-label-list/status-label-list.component').then(m => m.StatusLabelListComponent)
      },
      {
        path: 'status-labels/new',
        canActivate: [AdminGuard],
        loadComponent: () => import('./pages/status-labels/status-label-form/status-label-form.component').then(m => m.StatusLabelFormComponent)
      },
      {
        path: 'status-labels/:id/edit',
        canActivate: [AdminGuard],
        loadComponent: () => import('./pages/status-labels/status-label-form/status-label-form.component').then(m => m.StatusLabelFormComponent)
      },
      {
        path: 'locations',
        loadComponent: () => import('./pages/locations/location-list/location-list.component').then(m => m.LocationListComponent)
      },
      {
        path: 'locations/new',
        canActivate: [AdminGuard],
        loadComponent: () => import('./pages/locations/location-form/location-form.component').then(m => m.LocationFormComponent)
      },
      {
        path: 'locations/:id/edit',
        canActivate: [AdminGuard],
        loadComponent: () => import('./pages/locations/location-form/location-form.component').then(m => m.LocationFormComponent)
      },
      {
        path: 'departments',
        loadComponent: () => import('./pages/departments/department-list/department-list.component').then(m => m.DepartmentListComponent)
      },
      {
        path: 'departments/new',
        canActivate: [AdminGuard],
        loadComponent: () => import('./pages/departments/department-form/department-form.component').then(m => m.DepartmentFormComponent)
      },
      {
        path: 'departments/:id/edit',
        canActivate: [AdminGuard],
        loadComponent: () => import('./pages/departments/department-form/department-form.component').then(m => m.DepartmentFormComponent)
      },
      {
        path: 'companies',
        loadComponent: () => import('./pages/companies/company-list/company-list.component').then(m => m.CompanyListComponent)
      },
      {
        path: 'companies/new',
        canActivate: [AdminGuard],
        loadComponent: () => import('./pages/companies/company-form/company-form.component').then(m => m.CompanyFormComponent)
      },
      {
        path: 'companies/:id/edit',
        canActivate: [AdminGuard],
        loadComponent: () => import('./pages/companies/company-form/company-form.component').then(m => m.CompanyFormComponent)
      }
    ]
  },
  {
    path: '**',
    redirectTo: ''
  }
];
