import { Component, inject, signal, HostListener, computed, OnInit } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { DatePipe } from '@angular/common';
import { BasketStore } from '../../core/services/basket-store';
import { AuthService } from '../../core/services/auth';
import { NotificationService } from '../../core/services/notification';

@Component({
  selector: 'app-navbar',
  imports: [RouterLink, RouterLinkActive, DatePipe],
  templateUrl: './navbar.html',
  styleUrl: './navbar.scss',
})
export class Navbar implements OnInit {
  basketStore = inject(BasketStore);
  auth = inject(AuthService);
  notifService = inject(NotificationService);
  private router = inject(Router);

  menuOpen = signal(false);
  notifOpen = signal(false);

  readonly initials = computed(() => {
    const p = this.auth.profile();
    if (!p) return '?';
    const f = p.firstName?.[0] ?? '';
    const l = p.lastName?.[0] ?? '';
    return (f + l).toUpperCase() || '?';
  });

  readonly email = computed(() => this.auth.profile()?.email ?? '');

  get isAdmin(): boolean {
    return this.auth.isAdmin();
  }

  ngOnInit(): void {
    if (this.auth.isAuthenticated()) {
      this.notifService.init();
    }
  }

  toggleMenu(e: Event) {
    e.stopPropagation();
    this.menuOpen.update(v => !v);
    this.notifOpen.set(false);
  }

  toggleNotif(e: Event) {
    e.stopPropagation();
    this.notifOpen.update(v => !v);
    this.menuOpen.set(false);
    if (this.notifOpen()) {
      this.notifService.markAllRead();
    }
  }

  onNotifClick(link?: string) {
    this.notifOpen.set(false);
    if (link) this.router.navigateByUrl(link);
  }

  @HostListener('document:click')
  closeAll() {
    this.menuOpen.set(false);
    this.notifOpen.set(false);
  }
}
