import { Component, OnDestroy, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClientService } from '../../../core/services/http-client.service';
import { Subject, debounceTime, distinctUntilChanged, switchMap, take, takeUntil } from 'rxjs';
import { UserInfoDto } from '../../../models/Dtos';
import { ToastrService } from 'ngx-toastr';
import { NotificationService } from '../../../core/services/notification.service';
import { FriendRequestReceivedResponse } from '../../../models/ResponseModels';
import { HttpErrorResponse } from '@angular/common/http';
import { FormControl, ReactiveFormsModule } from '@angular/forms';
import { ErrorMessageService } from '../../../core/services/error-message.service';

@Component({
  selector: 'app-connections',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './connections.component.html',
  styleUrls: ['./connections.component.scss'],
})
export class ConnectionsComponent implements OnInit, OnDestroy {
  pendingRequests: UserInfoDto[] = [];
  suggestions: UserInfoDto[] = [];
  searchControl = new FormControl('', { nonNullable: true });
  ngUnsubscribe: Subject<void> = new Subject<void>();
  sentRequests: Set<string> = new Set<string>();

  constructor(
    private httpService: HttpClientService,
    private toastr: ToastrService,
    private notificationService: NotificationService,
    private errorMessageService: ErrorMessageService
  ) {}

  ngOnInit(): void {
    this.httpService
      .getFriendSuggestions()
      .pipe(take(1))
      .subscribe((res: UserInfoDto[]) => {
        this.suggestions = res;
      });

    this.searchControl.valueChanges
      .pipe(
        debounceTime(250),
        distinctUntilChanged(),
        switchMap((searchText) =>
          this.httpService.getFriendSuggestions(searchText ?? '')
        ),
        takeUntil(this.ngUnsubscribe)
      )
      .subscribe({
        next: (res: UserInfoDto[]) => {
          this.suggestions = res;
        },
        error: (err) => {
          this.toastr.error(
            this.errorMessageService.getFriendlyMessage(
              err,
              'Unable to search users right now.'
            )
          );
        },
      });

    this.loadFriendRequests();
    this.notificationService.friendRequestReceived$
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe((res: FriendRequestReceivedResponse) => {
        this.toastr.info(
          res?.message ?? 'You received a friend request.'
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
        this.toastr.error(
          this.errorMessageService.getFriendlyMessage(
            err,
            'Unable to send friend request. Please try again.'
          )
        );
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
          this.toastr.success(res?.message ?? "Friend request accepted.");
        },
        error: (err: HttpErrorResponse) => {
          this.toastr.error(
            this.errorMessageService.getFriendlyMessage(
              err,
              'Unable to accept friend request. Please try again.'
            )
          );
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
      .subscribe((res: UserInfoDto[]) => {
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
