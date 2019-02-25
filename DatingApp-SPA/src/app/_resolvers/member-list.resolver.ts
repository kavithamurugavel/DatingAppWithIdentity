import { Injectable } from '@angular/core';
import { User } from '../_models/user';
import { Resolve, Router, ActivatedRouteSnapshot } from '@angular/router';
import { UserService } from '../_services/user.service';
import { AlertifyService } from '../_services/alertify.service';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable()
// the Route Resolver is to prevent loading a null user on ngInit's loadUser (in member detail component)
// and in turn having to use a safe navigation operator ? on every property of user in the html
// resolver is that intermediate code, which can be executed when a link has been clicked and before a component is loaded.
// https://codeburst.io/understanding-resolvers-in-angular-736e9db71267
export class MemberListResolver implements Resolve<User[]> {
    // just some random defaults for testing
    pageNumber = 1;
    pageSize = 5;

    constructor(private userService: UserService, private router: Router,
        private alertify: AlertifyService) {}

    resolve(route: ActivatedRouteSnapshot): Observable<User[]> {
        // we don't have to subscribe here because router automatically does it
        return this.userService.getUsers(this.pageNumber, this.pageSize).pipe(
            catchError(error => {
                this.alertify.error('Problem retrieving data');
                this.router.navigate(['/home']); // rerouting back to home so that we don't loop infinitely to the members page
                return of(null); // returning observable of null
            })
        );
    }
}
