import { Injectable, inject, signal, computed } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import * as signalR from '@microsoft/signalr';
import { AuthService } from './auth';

export interface AppNotification {
  id: string;
  userId: string;
  icon: string;
  text: string;
  createdAt: string;
  unread: boolean;
  link?: string;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private http = inject(HttpClient);
  private auth = inject(AuthService);

  private _notifications = signal<AppNotification[]>([]);
  readonly notifications = this._notifications.asReadonly();
  readonly unreadCount = computed(() => this._notifications().filter(n => n.unread).length);

  private hubConnection: signalR.HubConnection | null = null;
  private apiUrl = '/api/notification';

  init(): void {
    const userId = this.auth.getCustomerId();
    if (!userId) return;

    this.loadHistory(userId);
    this.connectHub(userId);
  }

  private loadHistory(userId: string): void {
    this.http.get<AppNotification[]>(`${this.apiUrl}/notifications/${userId}`).subscribe({
      next: (list) => this._notifications.set(list),
      error: () => {}
    });
  }

  private connectHub(userId: string): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl(`/api/hubs/notifications?userId=${userId}`, {
        accessTokenFactory: () => this.auth.getToken() ?? '',
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.on('ReceiveNotification', (notification: AppNotification) => {
      this._notifications.update(list => [notification, ...list].slice(0, 20));
    });

    this.hubConnection.start().catch(() => {});
  }

  markAllRead(): void {
    const userId = this.auth.getCustomerId();
    if (!userId) return;

    this.http.post(`${this.apiUrl}/notifications/${userId}/read`, {}).subscribe();
    this._notifications.update(list => list.map(n => ({ ...n, unread: false })));
  }

  disconnect(): void {
    this.hubConnection?.stop();
  }
}
