import { Injectable } from "@angular/core";
import { ActivatedRouteSnapshot, MaybeAsync, RedirectCommand, Resolve, RouterStateSnapshot } from "@angular/router";
import { UserInfoDto } from "../../models/Dtos";
import { HttpClientService } from "../services/http-client.service";
import { FriendInfoService } from "../global/friend-info.service";
import { Observable, take, tap } from "rxjs";

@Injectable({providedIn: 'root'})
export class FriendInfoResolver implements Resolve<UserInfoDto[]>{

    constructor(private httpService: HttpClientService, private friendService: FriendInfoService) {
        
    }

    resolve(): Observable<UserInfoDto[]>{
        return this.httpService.getFriendInfos().pipe(take(1), tap(res => this.friendService.setFriendInfos(res)));
    }
    
}