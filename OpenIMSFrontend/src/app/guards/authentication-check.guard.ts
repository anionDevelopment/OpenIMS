import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { UserDataService } from '../services/user-data.service';
import { tap } from 'rxjs';

export const authenticationCheckGuard: CanActivateFn = () => {
  const userDataService: UserDataService = inject(UserDataService);
  return userDataService.userIsLoggedIn().pipe(tap((userIsLoggedIn) => {
    if (!userIsLoggedIn) {
      const router: Router = inject(Router);
      router.navigate(['']);
    }
    return userIsLoggedIn;
  }));
};
