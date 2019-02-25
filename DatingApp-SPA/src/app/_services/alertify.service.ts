import { Injectable } from '@angular/core';
declare let alertify: any; // instead of importing it, we are using the imported alertify from angular.json in a variable

// an alternate to adding the alertify.min.css in styles.css file and using it directly, we can also add it to angular.json (under scripts),
// write a service like this one and use the service methods in the other components

@Injectable({
  providedIn: 'root'
})
export class AlertifyService {

constructor() { }

// we will be providing the okCallBack function when we use this confirm method
confirm(message: string, okCallBack: () => any) {
  alertify.confirm(message, function(e) {
    // e is the click event for user's 'ok' click
    if (e) {
      okCallBack();
    } else {} // for cancel
  });
}

success(message: string) {
  alertify.success(message);
}

error(message: string) {
  alertify.error(message);
}

warning(message: string) {
  alertify.warning(message);
}

// default
message(message: string) {
  alertify.message(message);
}

}
