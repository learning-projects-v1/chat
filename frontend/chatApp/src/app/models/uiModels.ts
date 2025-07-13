import { Chat, locationDict, Reaction } from "./Dtos";

export interface NavItem {
  label: string;
  route: string;
  icon: string;
}

export type GroupedReactions = {
  title: string;
  reactions: Reaction[];
};

export class ChatUi implements Chat {
  id?: string | undefined;
  content: string = "";
  senderId: string = "";
  chatThreadId: string = "";
  isSeen?: boolean | undefined;
  replyToMessageId?: string | undefined;
  sentAt: Date = new Date();
  reactions?: Reaction[] | undefined;

  groupedReactions?: GroupedReactions[];
  reactLocations?: locationDict;

  /**
   *
   */
  constructor(chat: Chat) {
    this.chatThreadId = chat?.chatThreadId;
    this.content = chat?.content;
    this.id = chat?.id;
    this.isSeen = chat?.isSeen;
    this.replyToMessageId = chat?.replyToMessageId;
    this.sentAt = chat?.sentAt;
    this.senderId = chat?.senderId;
    this.reactions = chat?.reactions;
    this.GroupReactions(chat?.reactions);
  }

  addReaction(reaction: Reaction) {}

  removeReaction(reaction: Reaction) {}

  GroupReactions(reactionDtos?: Reaction[]){
    let groupedReactions: GroupedReactions[] = [];
    let positions: { [title: string]: number } = {};

    reactionDtos?.forEach((r) => {
      if (positions && positions[r.type]!= undefined) {
        const id = positions[r.type];
        groupedReactions[id].reactions.push(r);
      } else {
        positions[r.type] = groupedReactions.length;
        groupedReactions.push({ title: r.type, reactions: [r] });
      }
    });
    this.groupedReactions = groupedReactions;
    this.reactLocations = positions;
  }

  static GetAllChats(chats: Chat[]){
    let chatsUi : ChatUi[] = [];
    chats.forEach(c => chatsUi.push(new ChatUi(c)));
    return chatsUi;
  }
}
