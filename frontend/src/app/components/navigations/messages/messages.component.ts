import { CommonModule } from '@angular/common';
import { Component, Input, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { HttpClientService } from '../../../core/services/http-client.service';
import { Subject, takeUntil } from 'rxjs';
import { LatestMessage } from '../../../models/UserModels';

export interface ConnectedUser {
  userId: string;
  username: string;
  lastMessage: string;
  lastMessageTime: string; // ISO format
}

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  styleUrls: ['./messages.component.scss'],
  imports: [CommonModule],
})
export class MessagesComponent implements OnInit, OnDestroy {
  latestMessages: LatestMessage[] = [];
  ngUnsubscribe: Subject<void> = new Subject<void>();
  constructor(private router: Router, private httpService: HttpClientService) {}

  ngOnInit(): void {
    this.httpService
      .getLatestMessages()
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe(res => {
        this.latestMessages = res;
      });
  }

  goToChat(userId: string) {
    this.router.navigate(['/chat', userId]);
  }

  ngOnDestroy(): void {
    this.ngUnsubscribe.next();
    this.ngUnsubscribe.complete();
  }
}
