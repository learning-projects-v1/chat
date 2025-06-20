import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClientService } from '../../../core/services/http-client.service';
import { Subject, take, takeUntil } from 'rxjs';
import { User } from '../../../models/UserModels';
import { ToastrService } from 'ngx-toastr';
import { NotificationService } from '../../../core/services/notification.service';
import { FriendRequestReceivedResponse } from '../../../models/ResponseModels';
import { HttpErrorResponse } from '@angular/common/http';

@Component({
  selector: 'app-connections',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './connections.component.html',
  styleUrls: ['./connections.component.scss'],
})
export class ConnectionsComponent implements OnInit, OnDestroy {
  pendingRequests: User[] = [];
  suggestions: User[] = [];
  ngUnsubscribe: Subject<void> = new Subject<void>();
  sentRequests: Set<string> = new Set<string>();

  constructor(
    private httpService: HttpClientService,
    private toastr: ToastrService,
    private notificationService: NotificationService
  ) {}

  ngOnInit(): void {
    this.httpService
      .getFriendSuggestions()
      .pipe(take(1))
      .subscribe((res: User[]) => {
        this.suggestions = res;
      });

    this.loadFriendRequests();
    this.notificationService.friendRequestReceived$
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe((res: FriendRequestReceivedResponse) => {
        this.toastr.show(
          res?.message ?? 'Frontend: You received a friend request'
        );
        this.loadFriendRequests();
      });
  }

  sendRequest(receiverId: string) {
    if(this.sentRequests.has(receiverId)) return;
    this.httpService.sendFriendRequest(receiverId).subscribe({
      next: (res) => {
        this.toastr.success('Friend request sent!');
        this.sentRequests.add(receiverId);  
      },
      error: (err) => {
        this.toastr.error(err?.error?.message ?? "front-end: error")
      }
    });
  }

  acceptRequest(userId: string) {
    this.httpService
      .acceptFriendRequest(userId)
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe({
        next: (res: any) => {
          this.pendingRequests = this.pendingRequests.filter((u) => u.id !== userId);
          this.toastr.show(res?.message ?? "Friend request accepted");
        },
        error: (err: HttpErrorResponse) => {
          this.toastr.error(err?.error?.message ?? "front-end: some error occured");
        }
      });
  }

  declineRequest(userId: string) {
    console.log(`Declined request from ${userId}`);
    this.pendingRequests = this.pendingRequests.filter((u) => u.id !== userId);
  }

  loadFriendRequests() {
    this.httpService
      .getFriendRequests()
      .pipe(take(1))
      .subscribe((res: User[]) => {
        this.pendingRequests = res;
      });
  }

  isRequestSent(id: string){
    return this.sentRequests.has(id);
  }

  test(){
    this.toastr.show("HSDFDSF");
    this.toastr.success("successssssss!");
  }
  ngOnDestroy(): void {
    this.ngUnsubscribe.next();
    this.ngUnsubscribe.complete();
  }
}