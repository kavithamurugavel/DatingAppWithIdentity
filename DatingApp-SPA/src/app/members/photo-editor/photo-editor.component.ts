import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core';
import { Photo } from 'src/app/_models/photo';
import { FileUploader } from 'ng2-file-upload';
import { environment } from 'src/environments/environment';
import { AuthService } from 'src/app/_services/auth.service';
import { UserService } from 'src/app/_services/user.service';
import { AlertifyService } from 'src/app/_services/alertify.service';

@Component({
  selector: 'app-photo-editor',
  templateUrl: './photo-editor.component.html',
  styleUrls: ['./photo-editor.component.css']
})

export class PhotoEditorComponent implements OnInit {
  // this component is a child component to member-edit, so we use @Input
  @Input() photos: Photo[];
  // when we click on the Main button in the photo-editor (child), we also need to
  // change the display/main pic (which is in the member edit page, the parent component)
  // so we use @Output to facilitate this
  @Output() getMemberPhotoChange = new EventEmitter<string>();

  // the following photo uploader code is from: https://valor-software.com/ng2-file-upload/
  // with few changes (like initializing the uploader down below)
  uploader: FileUploader;
  hasBaseDropZoneOver = false;
  baseUrl = environment.apiUrl;
  currentMain: Photo;

  constructor(private authService: AuthService, private userService: UserService,
    private alertify: AlertifyService) { }

  ngOnInit() {
    this.initializeUploader();
  }


  fileOverBase(e: any): void {
    this.hasBaseDropZoneOver = e;
  }

  initializeUploader() {
    this.uploader = new FileUploader({
      url: this.baseUrl + 'users/' + this.authService.decodedToken.nameid + '/photos',
      authToken: 'Bearer ' + localStorage.getItem('token'),
      isHTML5: true,
      allowedFileType: ['image'],
      removeAfterUpload: true, // removing uploaded photo from upload queue
      autoUpload: false, // since we are having an upload button instead
      maxFileSize: 10 * 1024 * 1024 // 10 MB
    });

    // this is to overcome the CORS error that we get on photo upload
    // basically since we are not giving AllowCredentials in the UseCors part in Startup.cs
    // we need to explicitly specify withCredentials = false below for the CORS error to go away
    // review Section 11, Lecture 109 for details
    this.uploader.onAfterAddingFile = (file) => {file.withCredentials = false; };

    this.uploader.onSuccessItem = (item, response, status, headers) => {
      if (response) {
        // https://www.w3schools.com/js/js_json_parse.asp
        // A common use of JSON is to exchange data to/from a web server. When receiving data from a web server, the data
        // is always a string. Parse the data with JSON.parse(), and the data becomes a JavaScript object.
        const res: Photo = JSON.parse(response); // parse the photo as an object
        // building a photo object from the response of the server
        const photo = {
          id: res.id,
          url: res.url,
          dateAdded: res.dateAdded,
          description: res.description,
          isMain: res.isMain,
          isApproved: res.isApproved
        };
        this.photos.push(photo);
        // having setMainphoto called here so that when a new user tries to upload a photo
        // the member edit and the nav bar photos are updated automatically
        if (photo.isMain) {
          this.authService.changeMemberPhoto(photo.url);
          this.authService.currentUser.photoUrl = photo.url;
          localStorage.setItem('user', JSON.stringify(this.authService.currentUser));
        }
      }
    };
  }

  setMainPhoto(photo: Photo) {
    this.userService.setMainPhoto(this.authService.decodedToken.nameid, photo.id).subscribe(() => {
      // filter selects a subset of items from array and returns it as a new array, and in our case,
      // since it will be an array of 1, we get the element by specifying index 0
      this.currentMain = this.photos.filter(p => p.isMain === true)[0];
      this.currentMain.isMain = false;
      photo.isMain = true;
      this.authService.changeMemberPhoto(photo.url); // the value changed here will be reflected in both nav and member-edit components
      // since they are both subscribed to the BehaviorSubject observable from the authService

      // this.getMemberPhotoChange.emit(photo.url);

      // this is required so that the newly updated photo from the step above will be updated from the local storage
      this.authService.currentUser.photoUrl = photo.url;
      // overriding the local storage photo with the new user details (i.e. with the new main photo)
      localStorage.setItem('user', JSON.stringify(this.authService.currentUser));
    }, error => {
      this.alertify.error(error);
    });
  }

  deletePhoto(id: number) {
    // confirm has string and okCallBack as parameters
    // https://alertifyjs.com/confirm.html
    this.alertify.confirm('Are you sure you want to delete this photo?', () => {
      this.userService.deletePhoto(this.authService.decodedToken.nameid, id).subscribe(() => {
        // splice's first param: An integer that specifies at what position to add/remove items and second param: delete count
        // https://www.w3schools.com/jsref/jsref_splice.asp
        this.photos.splice(this.photos.findIndex(p => p.id === id), 1);
        this.alertify.success('Photo has been deleted');
      }, error => {
        this.alertify.error('Failed to delete the photo');
      });
    });
  }
}
