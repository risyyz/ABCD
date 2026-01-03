import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { combineLatest, filter, map, take, tap } from 'rxjs';
import { AuthService } from '../services/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  return combineLatest([
    authService.isAuthenticated$,
    authService.isAuthResolved$
  ]).pipe(
    filter(([_, resolved]) => resolved), // Wait until backend check is done
    take(1), // Only need the first value after resolved
    tap(([isAuthenticated]) => {
      if (!isAuthenticated) {
        router.navigate(['/auth/login']);
      }
    }),
    map(([isAuthenticated]) => isAuthenticated)
  );
};
