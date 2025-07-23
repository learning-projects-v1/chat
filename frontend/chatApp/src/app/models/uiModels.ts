import { Chat, locationDict, ReactionDto} from "./Dtos";

export interface NavItem {
  label: string;
  route: string;
  icon: string;
}

export type GroupedReactions = {
  title: string;
  reactions: ReactionDto[];
};

export class ChatUi implements Chat {
  id?: string | undefined;
  content: string = "";
  senderId: string = "";
  chatThreadId: string = "";
  isSeen?: boolean | undefined;
  replyToMessageId?: string | undefined;
  sentAt: Date = new Date();
  reactions?: ReactionDto[] | undefined;

  groupedReactions: GroupedReactions[] = [];
  reactLocations: locationDict = {};

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

  private addReaction(reaction: ReactionDto) {}

  private removeReaction(id: string) {

  }

  updateByLatestReaction(reaction: ReactionDto){
    const userReact = this.groupedReactions.find(g => g.reactions.find(r => r.senderId == reaction.senderId));
    if(userReact){
      let indx = userReact.reactions.findIndex(x => x.senderId == reaction.senderId);
      userReact.reactions.splice(indx, 1);
    }
    if(reaction.type){
      let indx = this.groupedReactions.findIndex(g => g.title == reaction.type);
      if(indx == -1){
        this.groupedReactions.push({title : reaction.type, reactions: []});
        indx = this.groupedReactions.length - 1;
      }
      this.groupedReactions[indx].reactions.push(reaction);
    }
  }

  GroupReactions(reactionDtos?: ReactionDto[]){
    reactionDtos?.forEach(r => {
      let index = this.groupedReactions.findIndex(x => x.title == r.type);
      if(index == -1){
        this.groupedReactions.push({title: r.type, reactions: []});
        index = this.groupedReactions.length - 1;
      }
      this.groupedReactions[index].reactions.push(r);
    })
  }
  // GroupReactions(reactionDtos?: Reaction[]){
  //   let groupedReactions: GroupedReactions[] = [];
  //   let positions: { [title: string]: number } = {};

  //   reactionDtos?.forEach((r) => {
  //     if (positions && positions[r.type]!= undefined) {
  //       const id = positions[r.type];
  //       groupedReactions[id].reactions.push(r);
  //     } else {
  //       positions[r.type] = groupedReactions.length;
  //       groupedReactions.push({ title: r.type, reactions: [r] });
  //     }
  //   });
  //   this.groupedReactions = groupedReactions;
  //   this.reactLocations = positions;
  // }

  static GetAllChats(chats: Chat[]){
    let chatsUi : ChatUi[] = [];
    chats.forEach(c => chatsUi.push(new ChatUi(c)));
    return chatsUi;
  }
}
