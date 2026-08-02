import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ToastService } from '../../core/services/toast';

@Component({
  selector: 'app-toast',
  imports: [CommonModule],
  template: `
    @if (toastService.toast()) {
      <div class="toast" [class.error]="toastService.toast()!.type === 'error'">
        {{ toastService.toast()!.message }}
      </div>
    }
  `,
  styles: [`
    .toast {
      position: fixed;
      bottom: 24px;
      right: 24px;
      background: #10b981;
      color: #fff;
      padding: 12px 20px;
      border-radius: 8px;
      font-size: 14px;
      font-weight: 500;
      z-index: 1000;
      animation: slideIn 0.2s ease;
      box-shadow: 0 4px 12px rgba(0,0,0,0.3);
    }
    .toast.error { background: #ef4444; }

    @keyframes slideIn {
      from { transform: translateY(20px); opacity: 0; }
      to   { transform: translateY(0);    opacity: 1; }
    }
  `]
})
export class Toast {
  toastService = inject(ToastService);
}
