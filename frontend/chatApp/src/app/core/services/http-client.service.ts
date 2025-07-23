import { HttpClient, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { environment } from "../../environments/environment";
import { LoginRequest, RegisterRequest } from "../../models/AuthModels";
import { apiEndpoints } from "../api-endpoints";
import { Observable } from "rxjs";
import { Chat, ChatOverview, ChatThread, ChatThreadPreview, UserInfoDto, User, ReactionDto } from "../../models/Dtos";

@Injectable({providedIn: "root"})
export class HttpClientService{   
    private accessToken: string | undefined;
    constructor(private http: HttpClient) {
        
    }

    setAccessToken(token: string) {
      this.accessToken = token;
    }

    getAccessToken(){
        return this.accessToken;
    }

    register(request : RegisterRequest){
        return this.http.post(apiEndpoints.Register, request);
    }

    login(request: LoginRequest){
        return this.http.post(apiEndpoints.Login, request);
    }


    getFriendSuggestions(): Observable<UserInfoDto[]> {
        return this.http.get<UserInfoDto[]>(apiEndpoints.FriendSuggestions, {});
    }

    
    getFriendRequests(){
        return this.http.get<UserInfoDto[]>(apiEndpoints.FriendRequests);
    }

    sendFriendRequest(receiverId: string){
        return this.http.post(apiEndpoints.SendFriendRequest(receiverId), null);
    }

    acceptFriendRequest(senderId: string){
        return this.http.put(apiEndpoints.AcceptFriendRequest(senderId), null);
    }

    
    getFriendInfos(){
        return this.http.get<UserInfoDto[]>(apiEndpoints.Friends);
    }

    getLatestMessages(){
        return this.http.get<Chat[]>(apiEndpoints.LatestMessages);
    }
 
    getThreadContents(threadId: string) {
      return this.http.get<ChatThread>(apiEndpoints.Thread(threadId));
    }

    sendMessage(messagePayload: Chat) {
        return this.http.post<Chat>(apiEndpoints.SendMessage(messagePayload.chatThreadId), messagePayload);
    }

    updateReact(reaction: ReactionDto){
        return this.http.post<ReactionDto>(apiEndpoints.UpdateReaction(reaction.threadId!, reaction.messageId!), reaction)    
    }

    addReact(reaction: ReactionDto){
        return this.http.post<ReactionDto>(apiEndpoints.AddReaction(reaction.threadId!, reaction.messageId!), reaction)    
    }

    deleteReact(reactId: string, threadId: string, messageId: string){
        return this.http.delete(apiEndpoints.DeleteReaction(reactId, threadId, messageId))
    }
    
    test(testMessage: string) {
        let  params = new HttpParams();
        params = params.append("message", testMessage);
        return this.http.get(apiEndpoints.Test, {params: params});
    }
}