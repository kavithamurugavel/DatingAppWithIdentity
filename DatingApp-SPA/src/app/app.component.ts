import { Component, OnInit } from '@angular/core';
import { AuthService } from './_services/auth.service';
import {JwtHelperService} from '@auth0/angular-jwt';
import { User } from './_models/user';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  jwtHelper = new JwtHelperService();
  // we are injecting authService here so that when the application loads, we will
  // get the token from the local storage and set the token inside auth service when our application first loads
  // this is to correctly display the name in Welcome *User*
  constructor(private authService: AuthService) {}

  ngOnInit() {
    const token = localStorage.getItem('token');
    // using JSON.parse to unstringify the string to object
    const user: User = JSON.parse(localStorage.getItem('user'));
    if (token) {
      // decoding the token from the local storage on initial load
      this.authService.decodedToken = this.jwtHelper.decodeToken(token);
    }
    if (user) {
      this.authService.currentUser = user;
      this.authService.changeMemberPhoto(user.photoUrl); // we use changeMemberPhoto here so that when the page
      // is refreshed/revisited, it gets the most recently locally stored value that is being set by the main photo updating steps
      // in photo-editor.component.ts
    }
  }
}
