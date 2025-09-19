import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

export interface NotificationMessage {
  message: string;
  type: 'success' | 'danger' | 'warning' | 'info';
  duration?: number;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notificationSubject = new Subject<NotificationMessage>();
  public notifications$ = this.notificationSubject.asObservable();

  showSuccess(message: string, duration?: number): void {
    this.notificationSubject.next({
      message,
      type: 'success',
      duration: duration || 3000
    });
  }

  showError(message: string, duration?: number): void {
    this.notificationSubject.next({
      message,
      type: 'danger',
      duration: duration || 5000
    });
  }

  showWarning(message: string, duration?: number): void {
    this.notificationSubject.next({
      message,
      type: 'warning',
      duration: duration || 4000
    });
  }

  showInfo(message: string, duration?: number): void {
    this.notificationSubject.next({
      message,
      type: 'info',
      duration: duration || 3000
    });
  }
}