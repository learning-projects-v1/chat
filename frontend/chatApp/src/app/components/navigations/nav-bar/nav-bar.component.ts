import { CommonModule } from '@angular/common';
import { Component, Input } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterLinkActive, RouterModule, Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { UserService } from '../../../core/services/auth.service';
import { HttpClientService } from '../../../core/services/http-client.service';
import { UserInfo } from '../../../models/AuthModels';
import { NavItem } from '../../../models/uiModels';
import { NotificationService } from '../../../core/services/notification.service';

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
  isLoggedIn = true;
  @Input() navItems: NavItem[] = [];

  constructor(
    private httpService: HttpClientService,
    private toastr: ToastrService,
    private userService: UserService,
    private router: Router,
    private notificationService: NotificationService
  ) { }

  ngOnInit(): void {
    this.userInfo = this.userService.getUserInfo();
  }

  logout() {
    this.httpService.setAccessToken('');
    this.notificationService.disconnect();
    this.toastr.info('Logged out');
    this.router.navigate(['/']);
  }
}
