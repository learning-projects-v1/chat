import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClientService } from '../../core/services/http-client.service';
import { ToastrService } from 'ngx-toastr';
import { UserInfo } from '../../models/AuthModels';
import { UserService } from '../../core/services/auth.service';
import { RouterLink, RouterLinkActive, RouterModule, RouterOutlet} from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-home',
  imports: [FormsModule, RouterModule, CommonModule],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent implements OnInit{
  testInput : string = "Input";
  testMessage: string="";
  userInfo: UserInfo | undefined;

  isLoggedIn = true; // Replace with actual auth logic
  username = 'John Doe';
  status = 'online'; // Can be 'offline'
  navItems = [
  { label: 'Connections', route: '/connections', icon: '🧠' }, // Suggested users
  { label: 'Messages', route: '/messages', icon: '💬' }, // Chat interface
  { label: 'Settings', route: '/settings', icon: '⚙️' }
];

  constructor(private httpService: HttpClientService, private toastr: ToastrService, private userService: UserService) {
    
  }

  ngOnInit(): void {
    this.userInfo = this.userService.getUserInfo()
  }

  
  test(){
    this.httpService.test(this.testInput).subscribe((res: any) => {
      this.testMessage = res?.message;
      this.toastr.show(res?.message ?? "Nothing found");
    })  
  }
 
  logout() {
    // this.isLoggedIn = false;
    // Add actual logout logic here
  }
    
}
