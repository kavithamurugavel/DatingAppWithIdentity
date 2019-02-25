import { Directive, Input, ViewContainerRef, TemplateRef, OnInit } from '@angular/core';
import { AuthService } from '../_services/auth.service';

@Directive({
  selector: '[appHasRole]'
})
export class HasRoleDirective implements OnInit {
  @Input() appHasRole: string[];
  isVisible = false; // to check if the element that we are showing or hiding is already visible

  // the view container could be a component or a template. We are using it as a template
  // when we use this directive with an *, it transforms the element into a template like *<ng-template>
  constructor(private viewContainerRef: ViewContainerRef, private templateRef: TemplateRef<any>,
    private authService: AuthService) { }

    ngOnInit() {
      const userRoles = this.authService.decodedToken.role as Array<string>;

      // if no roles then clear viewContainerRef
      if (!userRoles) {
        this.viewContainerRef.clear();
      }

      // if user has role needed then render the element
      if (this.authService.roleMatch(this.appHasRole)) {
        if (!this.isVisible) {
          this.isVisible = true;
          // templateRef refers to element that we are applying the structural directive to
          this.viewContainerRef.createEmbeddedView(this.templateRef);
        }
      } else {
        this.isVisible = false;
        this.viewContainerRef.clear();
      }
    }
}
