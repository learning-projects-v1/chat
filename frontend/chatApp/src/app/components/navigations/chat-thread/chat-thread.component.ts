import {
  AfterViewChecked,
  AfterViewInit,
  ChangeDetectorRef,
  Component,
  ElementRef,
  HostListener,
  NgZone,
  OnDestroy,
  OnInit,
  QueryList,
  viewChild,
  ViewChild,
  ViewChildren,
} from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { HttpClientService } from "../../../core/services/http-client.service";
import { UserService } from "../../../core/services/auth.service";
import { UserInfo } from "../../../models/AuthModels";
import { CommonModule } from "@angular/common";
import {
  ChatOverview,
  ChatThread,
  ChatThreadPreview,
  UserInfoDto,
  IncomingMessageNotification,
  User,
  Chat,
  IncomingReactionNotification,
  ReactionDto,
  SeenStatus,
} from "../../../models/Dtos";
import { FormsModule } from "@angular/forms";
import { NotificationService } from "../../../core/services/notification.service";
import {
  Subject,
  take,
  takeUntil,
  map,
  EMPTY,
  Observable,
  switchMap,
  concatMap,
  concat,
  of,
} from "rxjs";
import { ToastrService } from "ngx-toastr";
import { RouteConstants } from "../../../core/constants";
import { FriendInfoService } from "../../../core/global/friend-info.service";
import { ChatUi } from "../../../models/uiModels";
import { SeenObserverDirective } from "../../../seen-observer.directive";

interface reactionModal {
  class: string;
  title: string;
  style: string;
}

@Component({
  selector: "app-chat",
  templateUrl: "./chat-thread.component.html",
  styleUrls: ["./chat-thread.component.scss"],
  imports: [CommonModule, FormsModule],
})
export class ChatThreadComponent
  implements OnInit, AfterViewInit, OnDestroy
{
  @ViewChild("scrollAnchor") private scrollAnchor!: ElementRef;
  @ViewChild("messageInput") messageInputRef!: ElementRef<HTMLInputElement>;
  @ViewChild("scrollContainer") scrollContainer!: ElementRef;
  @ViewChildren("messageElement") messageElements!: QueryList<ElementRef>;

  threadId: string = "";
  friendId!: string;
  currentUserId!: string;
  currentUserInfo!: UserInfo;
  friendInfoList!: UserInfoDto[];
  chats: ChatUi[] = [];
  newMessage = "";
  ngUnsubscribe = new Subject<void>();
  userNameMap: { [key: string]: string } = {};
  isReplying: boolean = false;
  replyToMessage: Chat | null = null;
  reactToMessageId: string | null = "";
  reactionPopupPosition = { top: 0, left: 0 };
  reactionModals: reactionModal[] = [];
  friendInfosMap: UserInfoDto[] = [];
  reactionModalsDict: Map<string, reactionModal> = new Map();
  hasUpdatedSeenStatus: boolean = false;
  hoveredMessageId: string | null = null;
  hasScrolledInitially = false;

  constructor(
    private route: ActivatedRoute,
    private httpservice: HttpClientService,
    private userService: UserService,
    private notificationService: NotificationService,
    private toastr: ToastrService,
    private cdRef: ChangeDetectorRef,
    private friendInfoService: FriendInfoService,
    private zone: NgZone
  ) {
    this.reactionModals = [
      { class: "fas fa-thumbs-up", title: "Like", style: "color:#3b82f6;" },
      { class: "fas fa-heart", title: "Love", style: "color:#ef4444;" },
      { class: "fas fa-face-laugh", title: "Laugh", style: "color:#facc15;" },
      {
        class: "fas fa-face-surprise",
        title: "Surprised",
        style: "color:#8b5cf6;",
      },
      { class: "fas fa-face-sad-tear", title: "Sad", style: "color:#64748b;" },
    ];
    this.reactionModals.forEach((x) => this.reactionModalsDict.set(x.title, x));
  }

  ngOnInit(): void {
    this.threadId = this.route.snapshot.paramMap.get(
      RouteConstants.chatThreadParam
    )!;
    this.loadMessages(this.threadId);

    this.notificationService.messageReceived$
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe((incomingMessage: IncomingMessageNotification) => {
        if (this.chats.find((x) => x.id !== incomingMessage.chat.id)) {
          this.chats.push(new ChatUi(incomingMessage.chat));
        }
      });

    this.notificationService
      .reactionReceived(this.threadId)
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe((incomingReaction: IncomingReactionNotification) => {
        const chat = this.chats.find((x) => x.id == incomingReaction.messageId);
        chat?.updateByLatestReaction(incomingReaction);
      });

    this.currentUserInfo = this.userService.getUserInfo();
    this.currentUserId = this.currentUserInfo.UserId;
  }

  ngAfterViewInit(): void {
    this.messageElements.changes.subscribe(() => {
      if(!this.hasScrolledInitially){
        this.scrollToBottom();
        this.hasScrolledInitially = true;
      }
    });
    this.focusInput();
    document.addEventListener("click", this.handleDocumentClick);
  }

  loadMessages(threadId: string): void {
    this.httpservice
      .getThreadContents(threadId)
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe((data: ChatThread) => {
        this.chats = ChatUi.GetAllChats(data?.chats);
        this.friendInfoList = data?.memberInfoList;
        this.mapUserNames();
      });
  }

  //html
  onSendReplyMessage() {
    this.sendMessage();
  }

  //html
  onSendMessage() {
    this.sendMessage();
  }

  replyTo(msg: Chat) {
    this.replyToMessage = msg;

    setTimeout(() => {
      this.messageInputRef?.nativeElement.focus();
    }, 0.5);
  }

  focusInput() {
    // Timeout ensures focus after view updates
    setTimeout(() => {
      this.messageInputRef?.nativeElement?.focus();
    });
  }

  @HostListener("document:keydown", ["$event"])
  onGlobalKeydown(event: KeyboardEvent) {
    const input = this.messageInputRef?.nativeElement;
    const charCode = event.key.charCodeAt(0);
    const isTextChar =
      (event.key.length === 1 && charCode >= 65 && charCode <= 90) ||
      (charCode >= 97 && charCode <= 122);

    if (isTextChar && document.activeElement !== input) {
      input?.focus();
    }
  }

  cancelReply() {
    this.replyToMessage = null;
  }

  reactTo(event: reactionModal) {
    const react: ReactionDto = {
      senderId: this.currentUserId,
      type: event?.title,
      messageId: this.reactToMessageId!,
      threadId: this.threadId,
    };
    this.httpservice
      .updateReact(react)
      .pipe(takeUntil(this.ngUnsubscribe))
      .subscribe((res) => {
        /// react success
      });

    // const chat = this.chats.find((x) => x.id == this.reactToMessageId);
    // const currentUserReact = this.getUserReact(
    //   this.reactToMessageId!,
    //   this.currentUserId
    // );
    // let deleteReact$: Observable<any> = of(null);
    // let addReact$: Observable<any> = of(null);
    // if (currentUserReact) {
    //   deleteReact$ = this.deleteReact(chat!, currentUserReact);
    // }
    // if (!currentUserReact || currentUserReact?.type != react.type) {
    //   // add
    //   const reactDto = {
    //     ...react,
    //     messageId: this.reactToMessageId,
    //     threadId: this.threadId,
    //   } as ReactionDto;

    //   addReact$ = this.addReact(reactDto, chat!);
    // }
    // deleteReact$
    //   .pipe(
    //     takeUntil(this.ngUnsubscribe),
    //     concatMap(() => addReact$)
    //   )
    //   .subscribe(() => {});
  }

  getUserReact(messageId: string, userId: string) {
    let chat = this.chats.find((x) => x.id == messageId);
    const currentUserReact = chat?.groupedReactions
      .map((x) => x.reactions.find((y) => y.senderId == userId))
      .find((z) => z);

    return currentUserReact;
  }

  deleteMsg(event: any) {}

  sendMessage(): void {
    if (!this.newMessage.trim()) return;
    const messagePayload: Chat = {
      content: this.newMessage,
      senderId: this.currentUserId,
      sentAt: new Date(),
      chatThreadId: this.threadId,
    };

    if (this.replyToMessage) {
      messagePayload.replyToMessageId = this.replyToMessage.id;
      this.replyToMessage = null;
    }

    this.httpservice
      .sendMessage(messagePayload)
      .subscribe((sentMessage: Chat) => {
        this.newMessage = "";
        // this.chats.push(new ChatUi(sentMessage));
        // setTimeout(() => this.scrollToBottom(), 50);
      });
  }

  mapUserNames() {
    for (let friendInfo of this.friendInfoList) {
      this.userNameMap[friendInfo.id] = friendInfo.username;
    }

    this.userNameMap[this.currentUserId] = "You";
  }

  private scrollToBottom(): void {
    if (this.scrollAnchor) {
      this.scrollAnchor.nativeElement.scrollIntoView({ behavior: "smooth" });
    }
  }

  getMessageContent(messageId: string) {
    let chat = this.chats.find((c) => c.id == messageId); ///todo: need optimizations for old messages
    return chat?.content ?? "Message not found";
  }

  getMessageSenderName(messageId: string) {
    let chat = this.chats.find((c) => c.id == messageId);
    if (chat && chat.senderId) return this.userNameMap[chat.senderId];
    else return "User not found";
  }

  isSenderMe(id: string) {
    return this.currentUserId == id;
  }

  showReactionButtons(msg: Chat, event: MouseEvent) {
    setTimeout(() => {
      const modalHeight = 34;
      const modalWidth = 140;
      const target = (event.target as HTMLElement).closest("button");
      const rect = target?.getBoundingClientRect();
      this.reactionPopupPosition = {
        top: rect!.top - modalHeight - 5,
        left: rect!.left - modalWidth / 2,
      };
      this.reactToMessageId = msg.id ?? "";
    });
  }

  handleDocumentClick = (event: MouseEvent) => {
    const target = event.target as HTMLElement;
    const clickedInside = target.closest(".reaction-popup");
    if (!clickedInside) {
      this.reactToMessageId = null;
      this.cdRef.detectChanges();
    }
  };

  getTitles(reactions: ReactionDto[]): string {
    return reactions.map((r) => this.userNameMap[r.senderId]).join(",");
  }

  onScroll(event: Event): void {
    if (this.hasUpdatedSeenStatus) return;

    const target = event.target as HTMLElement;
    const scrollTop = target.scrollTop;
    const scrollHeight = target.scrollHeight;
    const clientHeight = target.clientHeight;

    const scrolledRatio = (scrollTop + clientHeight) / scrollHeight;
    if (scrolledRatio > 0.8) {
      this.hasUpdatedSeenStatus = true;
      this.httpservice
        .updateSeenStatus(
          this.threadId,
          this.chats.map((c) => c.id!)
        )
        .pipe(takeUntil(this.ngUnsubscribe))
        .subscribe((res) => {
          console.log("Updated: " + (res ?? "undefined"));
        });
    }
  }

  getSeenStatusesText(seenStatuses: SeenStatus[]) {
    return seenStatuses
      .map((x) => this.userNameMap[x.userId] ?? "not found")
      .join(",");
  }

  ngOnDestroy(): void {
    document.removeEventListener("click", this.handleDocumentClick);
    this.ngUnsubscribe.next();
    this.ngUnsubscribe.complete();
  }
}
