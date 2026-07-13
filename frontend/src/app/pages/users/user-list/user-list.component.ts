import { Component, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { MatTableModule } from '@angular/material/table';
import { MatPaginatorModule, PageEvent } from '@angular/material/paginator';
import { MatSortModule, Sort } from '@angular/material/sort';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatCardModule } from '@angular/material/card';
import { Subject } from 'rxjs';
import { debounceTime, distinctUntilChanged } from 'rxjs/operators';
import { UserService, User } from '../../../services/user.service';
import { AuthService } from '../../../services/auth.service';
import { NotificationService } from '../../../services/notification.service';
import { ConfirmService } from '../../../services/confirm.service';
import { PageQuery } from '../../../models/paged-result.model';

@Component({
  selector: 'app-user-list',
  standalone: true,
  imports: [
    CommonModule, RouterModule, FormsModule,
    MatTableModule, MatPaginatorModule, MatSortModule, MatButtonModule,
    MatIconModule, MatInputModule, MatFormFieldModule, MatSlideToggleModule,
    MatTooltipModule, MatProgressBarModule, MatCardModule
  ],
  templateUrl: './user-list.component.html',
  styleUrls: ['./user-list.component.css']
})
export class UserListComponent implements OnInit {
  displayedColumns = ['id', 'username', 'name', 'email', 'department', 'activated', 'actions'];
  users: User[] = [];
  loading = false;
  total = 0;

  // query state
  pageIndex = 0;
  pageSize = 25;
  search = '';
  sortBy = 'id';
  sortDir: 'asc' | 'desc' = 'asc';

  private searchInput$ = new Subject<string>();

  constructor(
    private userService: UserService,
    private authService: AuthService,
    private notify: NotificationService,
    private confirm: ConfirmService,
    private router: Router
  ) {}

  get isAdmin(): boolean {
    return this.authService.isAdmin();
  }

  ngOnInit(): void {
    this.searchInput$
      .pipe(debounceTime(350), distinctUntilChanged())
      .subscribe(term => {
        this.search = term;
        this.pageIndex = 0;
        this.load();
      });
    this.load();
  }

  private currentQuery(): PageQuery {
    return {
      skip: this.pageIndex * this.pageSize,
      take: this.pageSize,
      search: this.search || undefined,
      sortBy: this.sortBy,
      sortDir: this.sortDir
    };
  }

  load(): void {
    this.loading = true;
    this.userService.listUsers(this.currentQuery()).subscribe({
      next: (res) => {
        this.users = res.data || [];
        this.total = res.total || 0;
        this.loading = false;
      },
      error: () => {
        this.notify.error('Failed to load users');
        this.loading = false;
      }
    });
  }

  onSearch(term: string): void {
    this.searchInput$.next(term);
  }

  onSort(sort: Sort): void {
    this.sortBy = sort.active || 'id';
    this.sortDir = (sort.direction || 'asc') as 'asc' | 'desc';
    this.load();
  }

  onPage(e: PageEvent): void {
    this.pageIndex = e.pageIndex;
    this.pageSize = e.pageSize;
    this.load();
  }

  toggleActivation(user: User): void {
    const next = !user.activated;
    this.userService.setActivation(user.id, next).subscribe({
      next: (updated) => {
        user.activated = updated.activated;
        this.notify.success(next ? `Enabled ${user.username}` : `Disabled ${user.username}`);
      },
      error: (err) => this.notify.error(err.error?.message || 'Failed to update user status')
    });
  }

  async remove(user: User): Promise<void> {
    const ok = await this.confirm.confirmDelete(user.username || `user #${user.id}`);
    if (!ok) return;

    this.userService.deleteUser(user.id).subscribe({
      next: () => {
        this.notify.success(`Deleted ${user.username}`);
        this.load();
      },
      error: (err) => this.notify.error(err.error?.message || 'Failed to delete user')
    });
  }

  edit(user: User): void {
    this.router.navigate(['/users', user.id, 'edit']);
  }
}
