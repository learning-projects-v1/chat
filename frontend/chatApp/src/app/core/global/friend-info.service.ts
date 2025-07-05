import { Injectable, OnInit } from "@angular/core";
import { HttpClientService } from "../services/http-client.service";
import { NotificationService } from "../services/notification.service";
import { UserInfoDto } from "../../models/Dtos";

@Injectable({providedIn: 'root'})
export class FriendInfoService{
    private friendInfosMap = new Map<string, UserInfoDto>();

    constructor(private httpService: HttpClientService, private notificationService: NotificationService ) {
        
    }

    setFriendInfos(friendInfos: UserInfoDto[]){
        friendInfos.forEach(f => this.friendInfosMap.set(f.id, f));
    }

    setFriendInfo(friendInfo: UserInfoDto){
        this.friendInfosMap.set(friendInfo.id, friendInfo);
    }

    getFriendInfo(id: string){
        return this.friendInfosMap.get(id);
    }

    getFriendInfosMap(): Map<string, UserInfoDto> {
        return this.friendInfosMap;
    }

    getAllFriendInfos() : UserInfoDto[]{
        return Array.from(this.friendInfosMap.values());
    }

    clear(){
        this.friendInfosMap.clear();
    }
}