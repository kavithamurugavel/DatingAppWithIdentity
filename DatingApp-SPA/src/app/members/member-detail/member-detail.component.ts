import { Component, OnInit, ViewChild } from '@angular/core';
import { User } from 'src/app/_models/user';
import { UserService } from 'src/app/_services/user.service';
import { AlertifyService } from 'src/app/_services/alertify.service';
import { ActivatedRoute } from '@angular/router';
import { NgxGalleryOptions, NgxGalleryImage, NgxGalleryAnimation } from 'ngx-gallery';
import { TabsetComponent } from 'ngx-bootstrap';

@Component({
  selector: 'app-member-detail',
  templateUrl: './member-detail.component.html',
  styleUrls: ['./member-detail.component.css']
})
export class MemberDetailComponent implements OnInit {
  user: User;

  // this is for messages tab: https://valor-software.com/ngx-bootstrap/#/tabs#tabs-manual-select
  @ViewChild('memberTabs') memberTabs: TabsetComponent;

  // from ngx gallery: https://www.npmjs.com/package/ngx-gallery (section 9 lecture 91)
  galleryOptions: NgxGalleryOptions[];
  galleryImages: NgxGalleryImage[];

  // ActivatedRoute will give us access to the route i.e. members/4 for eg.
  constructor(private userService: UserService, private alertify: AlertifyService,
    private route: ActivatedRoute) { }

  ngOnInit() {
    // this.loadUser(); // this cannot be used without using ? in html, because on first load a null user will throw console errors
    this.route.data.subscribe(data => {
      this.user = data['user']; // this user is the 'user' from the resolve part in routes.ts
    });

    // Important! - the query params are accessed by subscribing to this.route.queryParams, as below,
    // whereas, the path variables are accessed by "this.route.snapshot.params"
    // https://stackoverflow.com/questions/47455734/how-get-query-parameters-from-url-in-angular-5
    this.route.queryParams.subscribe(params => {
      const selectedTab = params['tab']; // this coincides with the queryParams from messages.html. We get this from the query string
      this.memberTabs.tabs[selectedTab > 0 ? selectedTab : 0].active = true;
    });

    // check code snippets in: https://www.npmjs.com/package/ngx-gallery
    // config options for the gallery
    this.galleryOptions = [
      {
        width: '500px',
        height: '500px',
        imagePercent: 100,
        thumbnailsColumns: 4,
        imageAnimation: NgxGalleryAnimation.Slide,
        preview: false
      }
    ];
    this.galleryImages = this.getImages();
  }

  getImages() {
    const imageUrls = [];
    for (let i = 0; i < this.user.photos.length; i++) {
      imageUrls.push({
        small: this.user.photos[i].url,
        medium: this.user.photos[i].url,
        big: this.user.photos[i].url,
        description: this.user.photos[i].description
      });
    }
    return imageUrls;
  }

  // https://valor-software.com/ngx-bootstrap/#/tabs#tabs-manual-select
  selectTab(tabID: number) {
    this.memberTabs.tabs[tabID].active = true;
  }

  // getting the id from members/4 for eg.
  // the + will force the id to be considered as a number instead of a string
  // loadUser() {
  //   this.userService.getUser(+this.route.snapshot.params['id'])
  //   .subscribe((user: User) => {
  //     this.user = user;
  //   }, error => {
  //     this.alertify.error(error);
  //   });
  // }

}
