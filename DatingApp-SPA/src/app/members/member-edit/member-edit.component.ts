import { Component, OnInit, ViewChild, HostListener } from '@angular/core';
import { User } from 'src/app/_models/user';
import { ActivatedRoute } from '@angular/router';
import { AlertifyService } from 'src/app/_services/alertify.service';
import { NgForm } from '@angular/forms';
import { UserService } from 'src/app/_services/user.service';
import { AuthService } from 'src/app/_services/auth.service';

@Component({
  selector: 'app-member-edit',
  templateUrl: './member-edit.component.html',
  styleUrls: ['./member-edit.component.css']
})
export class MemberEditComponent implements OnInit {
  // https://blog.angular-university.io/angular-viewchild/ (esp. the part 'Using @ViewChild to inject a reference to a DOM element')
  @ViewChild('editForm') editForm: NgForm; // so that we have access to all the form methods
  user: User;
  photoUrl: string;

  // https://angular.io/api/core/HostListener
  // the following lines are to display a warning of 'unsaved changes' if we close the browser window
  // while editing. window:beforeunload is a browser event which is triggered right before actually unloading the page.
  @HostListener('window:beforeunload', ['$event'])
  unloadNotification($event: any) {
    // A Boolean value which is true if the event has not been canceled;
    // otherwise, if the event has been canceled or the default has been prevented, the value is false.
    if (this.editForm.dirty) {
      $event.returnValue = true;
    }
  }

  constructor(private route: ActivatedRoute, private alertify: AlertifyService, private userService: UserService,
    private authService: AuthService) { }

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.user = data['user'];
    });
    this.authService.currentPhotoUrl.subscribe(photoUrl => this.photoUrl = photoUrl);
  }

  updateUser() {
    this.userService.updateUser(this.authService.decodedToken.nameid, this.user).subscribe(next => {
      this.alertify.success('Profile updated successfully');
      // resetting form after saving changes so that it doesn't still isn't considered dirty
      // giving this.user so that the details are preserved on the page after resetting.
      // https://stackoverflow.com/questions/41500102/cleanest-way-to-reset-forms
      this.editForm.reset(this.user);
    }, error => {
      this.alertify.error(error);
    });
  }

  // this method is used to update the display pic acc. to which photo the user
  // sets as their main photo, the functionality of which is executed in the photo-editor
  // component, which is the child of member-edit component
  updateMainPhoto(photoUrl) {
    this.user.photoUrl = photoUrl;
  }
}
