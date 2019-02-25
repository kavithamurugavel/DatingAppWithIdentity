import { Component, OnInit } from '@angular/core';
import { User } from '../../_models/user';
import { UserService } from '../../_services/user.service';
import { AlertifyService } from '../../_services/alertify.service';
import { ActivatedRoute } from '@angular/router';
import { Pagination, PaginatedResult } from 'src/app/_models/pagination';

@Component({
  selector: 'app-member-list',
  templateUrl: './member-list.component.html',
  styleUrls: ['./member-list.component.css']
})
export class MemberListComponent implements OnInit {
  users: User[];
  user: User = JSON.parse(localStorage.getItem('user'));
  genderList = [{value: 'male', displayName: 'Males'}, {value: 'female', displayName: 'Females'}];
  userParams: any = {};

  // for the complete pagination workflow between SPA and API, check Important Points.txt
  pagination: Pagination;

  constructor(private userService: UserService, private alertify: AlertifyService,
    private route: ActivatedRoute) { }

  ngOnInit() {
    this.route.data.subscribe(data => {
      this.users = data['users'].result; // the users, which is stored in the result variable of PaginatedResult
      this.pagination = data['users'].pagination; // the paginated information
    });

    // setting default values for the user params here, so that it reflects what the API does.
    // These userParams will then be sent to the user.service's getUsers() (thru loadUsers() below)
    this.userParams.gender = this.user.gender === 'female' ? 'male' : 'female';
    this.userParams.minAge = 18;
    this.userParams.maxAge = 99;
    this.userParams.orderBy = 'lastActive';
  }

  // https://valor-software.com/ngx-bootstrap/#/pagination#page-changed-event
  pageChanged(event: any): void {
    // this currentPage is then sent to user service's getUsers via loadUsers in next line
    this.pagination.currentPage = event.page;
    this.loadUsers(); // loading the next paginated batch of users
  }

  resetFilters() {
    this.userParams.gender = this.user.gender === 'female' ? 'male' : 'female';
    this.userParams.minAge = 18;
    this.userParams.maxAge = 99;
    this.loadUsers();
  }

  loadUsers() {
    // users of type user[] is what we are returning from subscribe method,
    // and that we are assigning to this.users i.e. the users: User[] declared above.
    // This was changed to include paginated results and their params in section 14, lecture 141
    this.userService.getUsers(this.pagination.currentPage, this.pagination.itemsPerPage, this.userParams)
    .subscribe((res: PaginatedResult<User[]>) => {
      this.users = res.result;
      this.pagination = res.pagination;
    }, error => {
      this.alertify.error(error);
    });
  }

}
