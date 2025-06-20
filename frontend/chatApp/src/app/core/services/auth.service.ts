import { Injectable } from "@angular/core";
import { UserInfo } from "../../models/AuthModels";

@Injectable({providedIn: "root"})
export class UserService{
    private userInfo!: UserInfo;

    setUserInfo(info: UserInfo){
        this.userInfo = info;
    }

    getUserInfo(){
        return this.userInfo;
    }
}