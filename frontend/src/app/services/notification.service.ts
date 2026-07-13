import { Injectable, inject } from '@angular/core';
import { MatSnackBar } from '@angular/material/snack-bar';

/**
 * Thin wrapper over MatSnackBar so components emit consistent toast notifications
 * (success / error / info) without each re-configuring duration and styling.
 */
@Injectable({ providedIn: 'root' })
export class NotificationService {
  private readonly snackBar = inject(MatSnackBar);

  success(message: string): void {
    this.show(message, 'snack-success');
  }

  error(message: string): void {
    this.show(message, 'snack-error', 6000);
  }

  info(message: string): void {
    this.show(message, 'snack-info');
  }

  private show(message: string, panelClass: string, duration = 3500): void {
    this.snackBar.open(message, 'Dismiss', {
      duration,
      panelClass: [panelClass],
      horizontalPosition: 'right',
      verticalPosition: 'top'
    });
  }
}
