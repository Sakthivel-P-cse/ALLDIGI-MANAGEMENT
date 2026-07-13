import { Injectable, inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { firstValueFrom } from 'rxjs';
import {
  ConfirmDialogComponent,
  ConfirmDialogData
} from '../components/confirm-dialog/confirm-dialog.component';

/**
 * Opens the shared confirmation dialog and resolves to the user's choice, so
 * callers can `if (await confirm.ask(...))` instead of managing dialog refs.
 */
@Injectable({ providedIn: 'root' })
export class ConfirmService {
  private readonly dialog = inject(MatDialog);

  async ask(data: ConfirmDialogData): Promise<boolean> {
    const ref = this.dialog.open(ConfirmDialogComponent, {
      width: '420px',
      data
    });
    const result = await firstValueFrom(ref.afterClosed());
    return result === true;
  }

  /** Convenience for the common "delete X?" destructive confirmation. */
  confirmDelete(itemName: string): Promise<boolean> {
    return this.ask({
      title: 'Confirm delete',
      message: `Are you sure you want to delete "${itemName}"? This action cannot be undone.`,
      confirmText: 'Delete',
      cancelText: 'Cancel',
      destructive: true
    });
  }
}
