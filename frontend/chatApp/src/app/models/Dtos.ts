export interface User{
    id: string,
    userName: string,
    fullName?: string,
    avatarUrl?: string
}

export interface Chat {
  id?: string,
  content: string,
  senderId: string,
  chatThreadId: string,
  messageSeenStatuses?: SeenStatus[],
  replyToMessageId?: string,
  sentAt: Date, // ISO string, will be parsed to Date
  reactions?: ReactionDto[],
  chatTitle?: string
}


export interface ChatOverview {
  id?: string,
  content: string,
  senderId: string,
  threadId: string,
  replyToMessageId?: string,
  sentAt: Date,
}

export interface UserInfoDto {
  id: string,
  username: string,
  avatarUrl?: string,
  fullName?: string
}

export interface ChatThread{
  memberInfoList: UserInfoDto[],
  chats: Chat[]
}

export interface ChatThreadPreview{
  senderInfo: {[key: string]: User};
  chat: ChatOverview
}

export interface IncomingMessageNotification{
  friendInfo: UserInfoDto,
  chat: Chat
}

export interface ReactionDto{
  id?: string,
  type: string,
  senderId: string,
  threadId?: string,
  messageId?: string,
  UpdatedAt?: Date
}

export type IncomingReactionNotification = ReactionDto;

export type locationDict = {[title: string]: number }

export interface SeenStatus{
  id?: string,
  messageId: string,
  userId: string,
  seenAt: Date,
}