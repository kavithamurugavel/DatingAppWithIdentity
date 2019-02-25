import { Injectable } from '@angular/core';
import { User } from '../_models/user';
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/alertify.service';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { AuthService } from '../_services/auth.service';

@Injectable()
// for more detailed comments on resolvers, check the member-detail and member-list resolvers.
export class MemberEditResolver implements Resolve<User> {
    // we need access to the user's decoded token (for the id part in the edit url), so we are bringing in auth service
    // and using decodedtoken.nameid in the method below
    constructor(private userService: UserService, private router: Router,
        private alertify: AlertifyService, private authService: AuthService) {}

    resolve(route: ActivatedRouteSnapshot): Observable<User> {
        // the nameid is connected to the ClaimTypes.NameIdentifier (where we have the id of the user)
        // from AuthController in the API. https://fildev.net/2018/10/06/token-authentication-management-jwt-in-angular/
        return this.userService.getUser(this.authService.decodedToken.nameid).pipe(
            catchError(error => {
                this.alertify.error('Problem retrieving your data');
                this.router.navigate(['/members']);
                return of(null);
            })
        );
    }
}
