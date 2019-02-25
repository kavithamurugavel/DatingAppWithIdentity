import {Injectable} from '@angular/core';
import { CanDeactivate } from '@angular/router';
import { MemberEditComponent } from '../members/member-edit/member-edit.component';

@Injectable()

// Interface that a class can implement to be a guard deciding if a route can be deactivated.
// If all guards return true, navigation will continue. If any guard returns false, navigation will be cancelled.
// in other words, The guard gives you a chance to clean-up or ask the user's permission before navigating away from the current view.
// https://angular.io/guide/router#candeactivate-handling-unsaved-changes
export class PreventUnsavedChanges implements CanDeactivate<MemberEditComponent> {
    canDeactivate(component: MemberEditComponent) {
        if (component.editForm.dirty) {
            return confirm('Are you sure you want to continue? Any unsaved changes will be lost.');
        }
        return true;
    }
}
