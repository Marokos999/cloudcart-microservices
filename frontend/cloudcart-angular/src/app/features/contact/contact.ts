import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';

@Component({
  selector: 'app-contact',
  imports: [FormsModule],
  templateUrl: './contact.html',
  styleUrl: './contact.scss',
})
export class Contact {
  private http = inject(HttpClient);

  name = '';
  email = '';
  message = '';
  sent = signal(false);
  sending = signal(false);
  error = signal('');

  submit() {
    if (!this.name || !this.email || !this.message) return;

    this.sending.set(true);
    this.error.set('');

    this.http.post('/api/notification/notifications/contact', {
      name: this.name,
      email: this.email,
      message: this.message,
      to: 'marokos987@outlook.com',
    }).subscribe({
      next: () => {
        this.sent.set(true);
        this.sending.set(false);
      },
      error: () => {
        this.error.set('Failed to send message. Please try again.');
        this.sending.set(false);
      }
    });
  }
}
