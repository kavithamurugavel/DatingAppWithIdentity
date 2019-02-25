import { Injectable } from '@angular/core';
import { HttpInterceptor, HttpHandler, HttpEvent, HttpRequest, HttpErrorResponse, HTTP_INTERCEPTORS } from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError } from 'rxjs/operators';

@Injectable()
// https://angular.io/api/common/http/HttpInterceptor
// https://medium.com/@MetonymyQT/angular-http-interceptors-what-are-they-and-how-to-use-them-52e060321088
// https://angular.io/guide/rx-library - check Error Handling part
// basically we try to intercept the error from the API/server
// having the error code globally as an injectable so that we can prevent error handling code duplication
export class ErrorInterceptor implements HttpInterceptor {
    // takes req of type HttpRequest, next of type HttpHandler and returns an Observable of type HttpEvent
    // Most interceptors will transform the outgoing request before passing it to the
    // next interceptor in the chain, by calling next.handle(transformedReq).
    intercept(req: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        return next.handle(req).pipe(
            catchError(error => {
                if (error instanceof HttpErrorResponse) {
                    if (error.status === 401) {
                        return throwError(error.statusText);
                    }
                    const applicationError = error.headers.get('Application-Error');
                    if (applicationError) { // if anything is inside the application error
                        console.error(applicationError);
                        return throwError(applicationError);
                    }
                    // for other errors
                    const serverError = error.error; // this will go inside the HttpResponse
                    let modelStateErrors = '';
                    // model errors will be an object
                    if (serverError && typeof serverError === 'object') {
                         // loop thru the object keys
                        for (const key in serverError) {
                            if (serverError[key]) {
                                modelStateErrors += serverError[key] + '\n'; // adding the errors to the modelstateerrors
                            }
                        }
                    }
                    return throwError(modelStateErrors || serverError || 'Server Error');
                }
            })
        );
    }
}

export const ErrorInterceptorProvider = {
    provide: HTTP_INTERCEPTORS, // adding our custom interceptor to the angular HTTP Interceptors
    useClass: ErrorInterceptor,
    multi: true // we don't want to replace the existing interceptors, just add to the array of interceptors
};
