import { HttpClient } from '@angular/common/http';
import { computed, inject, Injectable, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { Permission, Session } from '../models';

const storageKey = 'inventory.session';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly currentSession = signal<Session | null>(readStoredSession());

  readonly session = this.currentSession.asReadonly();
  readonly isAuthenticated = computed(() => this.currentSession() !== null);

  login(username: string, password: string): Observable<Session> {
    return this.http.post<Session>('/api/auth/login', { username, password }).pipe(
      tap((session) => {
        localStorage.setItem(storageKey, JSON.stringify(session));
        this.currentSession.set(session);
      }),
    );
  }

  logout(): void {
    localStorage.removeItem(storageKey);
    this.currentSession.set(null);
  }

  // Serve apenas para adaptar a tela: quem garante a permissão é a API.
  hasPermission(permission: Permission): boolean {
    return this.currentSession()?.permissions.includes(permission) ?? false;
  }
}

function readStoredSession(): Session | null {
  try {
    const stored = localStorage.getItem(storageKey);
    if (!stored) {
      return null;
    }

    const session = JSON.parse(stored) as Session;
    return Date.parse(session.expiresAt) > Date.now() ? session : null;
  } catch {
    return null;
  }
}
