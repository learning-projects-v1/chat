import { HttpClient, HttpParams } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { environment } from "../../environments/environment";
import { LoginRequest, RegisterRequest } from "../../models/AuthModels";
import { apiEndpoints } from "../api-endpoints";
import { Observable } from "rxjs";
import { Chat, ChatThread, ChatThreadPreview, User } from "../../models/UserModels";

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


    getFriendSuggestions(): Observable<User[]> {
        return this.http.get<User[]>(apiEndpoints.FriendSuggestions, {});
    }

    
    getFriendRequests(){
        return this.http.get<User[]>(apiEndpoints.FriendRequests);
    }

    sendFriendRequest(receiverId: string){
        return this.http.post(apiEndpoints.SendFriendRequest(receiverId), null);
    }

    acceptFriendRequest(senderId: string){
        return this.http.put(apiEndpoints.AcceptFriendRequest(senderId), null);
    }

    
    getFriends(){
        return this.http.get<User[]>(apiEndpoints.Friends);
    }

    getLatestMessages(){
        return this.http.get<ChatThreadPreview[]>(apiEndpoints.LatestMessages);
    }
 
    getChatHistory(friendId: string) {
      return this.http.get<ChatThread>(apiEndpoints.ChatHistory(friendId));
    }

    sendMessage(messagePayload: Chat) {
        return this.http.post<Chat>(apiEndpoints.SendMessage, messagePayload);
    } 
    
    test(testMessage: string) {
        let  params = new HttpParams();
        params = params.append("message", testMessage);
        return this.http.get(apiEndpoints.Test, {params: params});
    }
}