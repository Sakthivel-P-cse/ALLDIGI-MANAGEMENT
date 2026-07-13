import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { MatTabsModule } from '@angular/material/tabs';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatChipsModule } from '@angular/material/chips';
import { MatListModule } from '@angular/material/list';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { UserService, User, AssignedItems } from '../../../services/user.service';
import { AuthService } from '../../../services/auth.service';
import { NotificationService } from '../../../services/notification.service';

@Component({
  selector: 'app-user-detail',
  standalone: true,
  imports: [
    CommonModule, RouterModule,
    MatTabsModule, MatCardModule, MatButtonModule, MatIconModule,
    MatChipsModule, MatListModule, MatProgressBarModule
  ],
  templateUrl: './user-detail.component.html',
  styleUrls: ['./user-detail.component.css']
})
export class UserDetailComponent implements OnInit {
  user: User | null = null;
  items: AssignedItems | null = null;
  loading = false;

  constructor(
    private route: ActivatedRoute,
    private userService: UserService,
    private authService: AuthService,
    private notify: NotificationService
  ) {}

  get isAdmin(): boolean {
    return this.authService.isAdmin();
  }

  ngOnInit(): void {
    const idStr = this.route.snapshot.paramMap.get('id');
    if (!idStr) return;
    const id = parseInt(idStr, 10);
    this.loadUser(id);
    this.loadItems(id);
  }

  private loadUser(id: number): void {
    this.loading = true;
    this.userService.getUserById(id).subscribe({
      next: (u) => {
        this.user = u;
        this.loading = false;
      },
      error: () => {
        this.notify.error('Failed to load user details');
        this.loading = false;
      }
    });
  }

  private loadItems(id: number): void {
    this.userService.getUserItems(id).subscribe({
      next: (items) => (this.items = items),
      error: () => this.notify.error('Failed to load assigned items')
    });
  }

  resetPassword(): void {
    if (!this.user) return;
    const pwd = window.prompt(`Enter a new password for ${this.user.username} (min 6 characters):`);
    if (!pwd) return;
    if (pwd.length < 6) {
      this.notify.error('Password must be at least 6 characters');
      return;
    }
    this.userService.resetPassword(this.user.id, pwd, pwd).subscribe({
      next: () => this.notify.success('Password reset'),
      error: (err) => this.notify.error(err.error?.message || 'Failed to reset password')
    });
  }
}
