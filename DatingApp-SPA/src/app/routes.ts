import {Routes} from '@angular/router';
import { HomeComponent } from './home/home.component';
import { MessagesComponent } from './messages/messages.component';
import { MemberListComponent } from './members/member-list/member-list.component';
import { ListsComponent } from './lists/lists.component';
import { AuthGuard } from './_guards/auth.guard';
import { MemberDetailComponent } from './members/member-detail/member-detail.component';
import { MemberDetailResolver } from './_resolvers/member-detail.resolver';
import { MemberListResolver } from './_resolvers/member-list.resolver';
import { MemberEditComponent } from './members/member-edit/member-edit.component';
import { MemberEditResolver } from './_resolvers/member-edit.resolver';
import { PreventUnsavedChanges } from './_guards/prevent-unsaved-changes.guard';
import { ListsResolver } from './_resolvers/lists.resolver';
import { MessagesResolver } from './_resolvers/messages.resolver';
import { AdminPanelComponent } from './admin/admin-panel/admin-panel.component';

// array of Routes and each route is an object
// ordering of the routes is important, for eg: we shouldn't have the wildcard route line first
// because it would just look at the wildcard route and ignore the other routes
export const appRoutes: Routes = [
    // instead of giving path: 'home' we give an empty string so that home means localhost:4200 and not localhost:4200/home
    {path: '', component: HomeComponent},
    // creating a dummy route and include child routes for that, so that we can
    // use guard the routes using canActivate: [AuthGuard] in one place rather than replicate that line many times
    {
        path: '', // so that the url will be localhost:5000/members instead of localhost:5000/*somestring*members
        runGuardsAndResolvers: 'always',
        canActivate: [AuthGuard], // adding our can activate route guard here
        children: [
            {path: 'members', component: MemberListComponent, resolve: {users: MemberListResolver}},
            // resolve part is seen in section 9 lecture 90. resolve key holds another object where you
            // will define a key and assign your resolver service as a value to it.
            {path: 'members/:id', component: MemberDetailComponent, resolve: {user: MemberDetailResolver}},
            {path: 'member/edit', component: MemberEditComponent,
                resolve: {user: MemberEditResolver}, canDeactivate: [PreventUnsavedChanges]},
            {path: 'messages', component: MessagesComponent, resolve: {messages: MessagesResolver}},
            {path: 'lists', component: ListsComponent, resolve: {users: ListsResolver}},
            // The data property in the third route is a place to store arbitrary data associated with this specific route.
            // The data property is accessible within each activated route. Use it to store items such as page titles,
            // breadcrumb text, and other read-only, static data.
            // https://angular.io/guide/router#configuration
            {path: 'admin', component: AdminPanelComponent, data: {roles: ['Admin', 'Moderator']}},
        ]
    },
    // the following will be a wildcard route
    // pathMatch full means we are asking to match the full path of the url to redirect to home
    // instead of giving redirectTo: 'home' we give an empty string so that home means localhost:4200 and not localhost:4200/home
    {path: '**', redirectTo: '', pathMatch: 'full'}
];
