import { RegisterComponent } from './components/auth/register/register.component';
import { LoginComponent } from './components/auth/login/login.component';
import { NotFoundComponent } from './components/not-found/not-found.component';
import { Routes } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { ConnectionsComponent } from './components/navigations/connections/connections.component';
import { MessagesComponent } from './components/navigations/messages/messages.component';
import { ChatThreadComponent } from './components/navigations/chat-thread/chat-thread.component';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  { path: 'register', component: RegisterComponent },
  { path: 'login', component: LoginComponent },
  { path: 'home', component: HomeComponent },
  { path: 'connections', component: ConnectionsComponent },
  { path: 'messages', component: MessagesComponent },
  { path: 'chat/:friendId', component: ChatThreadComponent },
  { path: '**', component: NotFoundComponent },
];
