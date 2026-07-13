import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatCheckboxModule } from '@angular/material/checkbox';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { UserService, CreateUserPayload, UpdateUserPayload } from '../../../services/user.service';
import { NotificationService } from '../../../services/notification.service';

@Component({
  selector: 'app-user-form',
  standalone: true,
  imports: [
    CommonModule, ReactiveFormsModule,
    MatFormFieldModule, MatInputModule, MatButtonModule, MatCheckboxModule, MatProgressBarModule
  ],
  templateUrl: './user-form.component.html',
  styleUrls: ['./user-form.component.css']
})
export class UserFormComponent implements OnInit {
  userForm: FormGroup;
  loading = false;
  isEdit = false;
  userId: number | null = null;
  pageTitle = 'CREATE NEW USER';
  buttonText = 'SAVE USER';

  constructor(
    private fb: FormBuilder,
    private userService: UserService,
    private notify: NotificationService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.userForm = this.fb.group({
      username: ['', Validators.required],
      firstName: [''],
      lastName: [''],
      email: ['', [Validators.email]],
      jobtitle: [''],
      phone: [''],
      vip: [false],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  ngOnInit(): void {
    const idParam = this.route.snapshot.paramMap.get('id');
    if (idParam) {
      this.isEdit = true;
      this.userId = parseInt(idParam, 10);
      this.pageTitle = 'EDIT USER';
      this.buttonText = 'UPDATE USER';
      // Password is optional in edit mode (use the reset-password action to change it).
      this.userForm.get('password')?.clearValidators();
      this.userForm.get('password')?.updateValueAndValidity();
      this.loadUser(this.userId);
    }
  }

  private loadUser(id: number): void {
    this.loading = true;
    this.userService.getUserById(id).subscribe({
      next: (u) => {
        this.userForm.patchValue({
          username: u.username,
          firstName: u.firstName,
          lastName: u.lastName,
          email: u.email,
          jobtitle: u.jobtitle,
          phone: u.phone,
          vip: u.vip ?? false
        });
        this.loading = false;
      },
      error: () => {
        this.notify.error('Failed to load user');
        this.loading = false;
      }
    });
  }

  onSubmit(): void {
    if (this.userForm.invalid) {
      this.userForm.markAllAsTouched();
      return;
    }
    this.loading = true;
    const v = this.userForm.value;

    if (this.isEdit && this.userId != null) {
      const payload: UpdateUserPayload = {
        username: v.username,
        firstName: v.firstName,
        lastName: v.lastName,
        email: v.email,
        jobtitle: v.jobtitle,
        phone: v.phone,
        vip: v.vip
      };
      this.userService.updateUser(this.userId, payload).subscribe({
        next: () => {
          this.notify.success('User updated');
          this.router.navigate(['/users']);
        },
        error: (err) => {
          this.loading = false;
          this.notify.error(err.error?.message || 'Failed to update user');
        }
      });
    } else {
      const payload: CreateUserPayload = {
        username: v.username,
        firstName: v.firstName,
        lastName: v.lastName,
        email: v.email,
        jobtitle: v.jobtitle,
        phone: v.phone,
        vip: v.vip,
        password: v.password,
        activated: true
      };
      this.userService.createUser(payload).subscribe({
        next: () => {
          this.notify.success('User created');
          this.router.navigate(['/users']);
        },
        error: (err) => {
          this.loading = false;
          this.notify.error(err.error?.message || 'Failed to create user');
        }
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/users']);
  }
}
