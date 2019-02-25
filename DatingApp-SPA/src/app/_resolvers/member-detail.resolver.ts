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
// In our case it is to prevent loading a null user.
// https://codeburst.io/understanding-resolvers-in-angular-736e9db71267
export class MemberDetailResolver implements Resolve<User> {
    constructor(private userService: UserService, private router: Router,
        private alertify: AlertifyService) {}

    resolve(route: ActivatedRouteSnapshot): Observable<User> {
        // we don't have to subscribe here because router automatically does it
        return this.userService.getUser(route.params['id']).pipe(
            catchError(error => {
                this.alertify.error('Problem retrieving data');
                this.router.navigate(['/members']); // rerouting back to members
                return of(null); // returning observable of null. Of just emits the values specified, in this case, null. Check link
                // https://stackoverflow.com/questions/47889210/why-we-should-use-rxjs-of-function for more info on of()
            })
        );
    }
}
