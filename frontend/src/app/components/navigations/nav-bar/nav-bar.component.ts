import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterOutlet, RouterLinkActive, RouterModule } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { UserService } from '../../../core/services/auth.service';
import { HttpClientService } from '../../../core/services/http-client.service';
import { UserInfo } from '../../../models/AuthModels';
import { NavItem } from '../../../models/uiModels';

@Component({
  selector: 'app-nav-bar',
  imports: [
    FormsModule,
    RouterLinkActive,
    RouterModule,
    CommonModule,
  ],
  templateUrl: './nav-bar.component.html',
  styleUrl: './nav-bar.component.scss',
})
export class NavBarComponent {
  userInfo: UserInfo | undefined;
  isLoggedIn = true; // Replace with actual auth logic
  @Input() navItems :NavItem[] = [];

  constructor(
    private httpService: HttpClientService,
    private toastr: ToastrService,
    private userService: UserService
  ) {}

  ngOnInit(): void {
    this.userInfo = this.userService.getUserInfo();
  }

  logout() {
    // this.isLoggedIn = false;
    // Add actual logout logic here
  }
}
