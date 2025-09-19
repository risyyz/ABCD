import { Component, OnInit } from '@angular/core';
import { NotificationService, NotificationMessage } from '../services/notification.service';

@Component({
  selector: 'app-notification',
  standalone: false,
  template: `
    <div class="notification-container" *ngFor="let notification of notifications; trackBy: trackByIndex">
      <div class="notification" [ngClass]="'is-' + notification.type">
        <button class="delete" (click)="removeNotification(notification)"></button>
        {{ notification.message }}
      </div>
    </div>
  `,
  styles: [`
    .notification-container {
      position: fixed;
      top: 20px;
      right: 20px;
      z-index: 1000;
      max-width: 400px;
      margin-bottom: 10px;
    }
    
    .notification {
      margin-bottom: 10px;
      box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
      animation: slideIn 0.3s ease-out;
    }
    
    @keyframes slideIn {
      from {
        transform: translateX(100%);
        opacity: 0;
      }
      to {
        transform: translateX(0);
        opacity: 1;
      }
    }
  `]
})
export class NotificationComponent implements OnInit {
  notifications: NotificationMessage[] = [];

  constructor(private notificationService: NotificationService) {}

  ngOnInit(): void {
    this.notificationService.notifications$.subscribe(notification => {
      this.notifications.push(notification);
      
      // Auto-remove notification after duration
      if (notification.duration) {
        setTimeout(() => {
          this.removeNotification(notification);
        }, notification.duration);
      }
    });
  }

  removeNotification(notification: NotificationMessage): void {
    const index = this.notifications.indexOf(notification);
    if (index > -1) {
      this.notifications.splice(index, 1);
    }
  }

  trackByIndex(index: number): number {
    return index;
  }
}