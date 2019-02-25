import { Injectable } from '@angular/core';
import { environment } from 'src/environments/environment';
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { User } from '../_models/user';
import { PaginatedResult } from '../_models/pagination';
import { map } from 'rxjs/operators';
import { Message } from '../_models/message';

// the following is to send the http/API request with the token so that the API calls
// can be successful. The Authorization/Bearer part we have already seen in Postman
// this was replaced by angular jwt token part (section 9 lecture 86) with the jwt config
// code in app.module.ts
// const httpOptions = {
//   headers: new HttpHeaders({
//     'Authorization': 'Bearer ' + localStorage.getItem('token')
//   })
// };

@Injectable({
  providedIn: 'root'
})
export class UserService {
  baseUrl = environment.apiUrl; // getting from environment.ts

constructor(private http: HttpClient) { }

getUsers(page?, itemsPerPage?, userParams?, likesParam?): Observable<PaginatedResult<User[]>> {
  // since PaginatedResult is a class, we have to create a new instance here
  const paginatedResult: PaginatedResult<User[]> = new PaginatedResult<User[]>();

  // The url query string parameters are added using the helper class HttpParams
  // https://www.tektutorialshub.com/angular/angular-pass-url-parameters-query-strings/#HttpParams
  let params = new HttpParams();

  // this check is technically not reqd. since our API sends pageNumber = 1 and itemsPerPage = 10 by default
  if (page != null && itemsPerPage != null) {
    params = params.append('pageNumber', page);
    params = params.append('pageSize', itemsPerPage);
  }

  // for filtering section 14 lecture 144
  if (userParams != null) {
    params = params.append('minAge', userParams.minAge);
    params = params.append('maxAge', userParams.maxAge);
    params = params.append('gender', userParams.gender);
    params = params.append('orderBy', userParams.orderBy);
  }

  // appending the likes param to the query string sec 15, lec 154
  if (likesParam === 'Likers') {
    params = params.append('likers', 'true');
  }

  if (likesParam === 'Likees') {
    params = params.append('likees', 'true');
  }

  // get returns type of Observable of objects. So we need to cast it to User[]
  // the foll. was commented due to adding jwt token part in app.module.ts
  // return this.http.get<User[]>(this.baseUrl + 'users', httpOptions);

  // observe: Tell HttpClient that you want the full response with the observe option.
  // https://angular.io/guide/http#reading-the-full-response
  // We are including the response headers & HttpParam in get() to
  // get the users as well as the pagination information from the response headers and store them in our PaginatedResult class.
  return this.http.get<User[]>(this.baseUrl + 'users', {observe: 'response', params})
  .pipe (
    map(response => {
      paginatedResult.result = response.body; // these will be the users
      if (response.headers.get('Pagination') != null) { // if the Pagination header from the API is not null
        // converting serialized string format of the headers into Json object here
        paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
      }
      return paginatedResult;
    })
  );
}

getUser(id): Observable<User> {
  // the foll. was commented due to adding jwt token part in app.module.ts
  // return this.http.get<User>(this.baseUrl + 'users/' + id, httpOptions);
  return this.http.get<User>(this.baseUrl + 'users/' + id);
}

updateUser(id: number, user: User) {
  // PUT: update the user on the server. Returns the updated user upon success.
  return this.http.put(this.baseUrl + 'users/' + id, user);
}

// post requires a body so we are just giving an empty object {} to satisfy that
setMainPhoto(userID: number, id: number) {
  return this.http.post(this.baseUrl + 'users/' + userID + '/photos/' + id + '/setMain', {});
}

deletePhoto(userID: number, id: number) {
  return this.http.delete(this.baseUrl + 'users/' + userID + '/photos/' + id);
}

// the API call from this method will first check to see if a like is already in place for this user and recipient ids
// if not, it will log the like in the Likes table
sendLike(id: number, recipientId: number) {
  return this.http.post(this.baseUrl + 'users/' + id + '/like/' + recipientId, {});
}

getMessages(id: number, page?, itemsPerPage?, messageContainer?) {
  const paginatedResult: PaginatedResult<Message[]> = new PaginatedResult<Message[]>();

  let params = new HttpParams();

  params = params.append('MessageContainer', messageContainer);

  if (page != null && itemsPerPage != null) {
    params = params.append('pageNumber', page);
    params = params.append('pageSize', itemsPerPage);
  }
  // similar to the users paginated result return statement
  return this.http.get<Message[]>(this.baseUrl + 'users/' + id + '/messages', {observe: 'response', params})
      .pipe (
        map(response => {
          paginatedResult.result = response.body; // list of messages
          if (response.headers.get('Pagination') !== null) {
            paginatedResult.pagination = JSON.parse(response.headers.get('Pagination'));
          }

          return paginatedResult;
        })
      );
}

// id is the currently logged in user
getMessageThread(id: number, recipientId: number) {
  return this.http.get<Message[]>(this.baseUrl + 'users/' + id + '/messages/thread/' + recipientId);
}

sendMessage(id: number, message: Message) {
  return this.http.post(this.baseUrl + 'users/' + id + '/messages', message);
}

deleteMessage(id: number, userID: number) {
  return this.http.post(this.baseUrl + 'users/' + userID + '/messages/' + id, {});
}

markAsRead(userID: number, messageID: number) {
  // subscribing here directly because we are not sending anything back
  this.http.post(this.baseUrl + 'users/' + userID + '/messages/' + messageID + '/read', {})
  .subscribe();
}
}
