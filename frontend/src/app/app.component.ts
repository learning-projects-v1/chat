import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { NavigationEnd, Router, RouterOutlet } from '@angular/router';
import { NavBarComponent } from './components/navigations/nav-bar/nav-bar.component';
import { NavItem } from './models/uiModels';
import { filter } from 'rxjs';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, FormsModule, NavBarComponent],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent {
  title = 'chatApp';
  showNavBar = true;
  navItems : NavItem[] = [
    { label: 'Connections', route: '/connections', icon: '🧠' }, // Suggested users
    { label: 'Messages', route: '/messages', icon: '💬' }, // Chat interface
    { label: 'Settings', route: '/settings', icon: '⚙️' },
  ];

  constructor(private router: Router) {
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe((event: NavigationEnd) => {
        this.showNavBar = !['/login', '/register'].includes(event.urlAfterRedirects);
      })
  }
}
