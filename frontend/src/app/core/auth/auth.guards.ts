import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Permission } from '../models';
import { AuthService } from './auth.service';

export const authGuard: CanActivateFn = () => {
  return inject(AuthService).isAuthenticated() || inject(Router).createUrlTree(['/login']);
};

export const guestGuard: CanActivateFn = () => {
  return !inject(AuthService).isAuthenticated() || inject(Router).createUrlTree(['/']);
};

export function permissionGuard(permission: Permission): CanActivateFn {
  return () => inject(AuthService).hasPermission(permission) || inject(Router).createUrlTree(['/']);
}
