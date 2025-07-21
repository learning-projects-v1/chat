import { Injectable } from "@angular/core";
import { locationDict, ReactionDto } from "../../models/Dtos";
import { GroupedReactions } from "../../models/uiModels";

@Injectable({providedIn: 'root'})
export class UtilityService{
    constructor(){}

    /** returns array of grouped reactions and all positions of each reaction indexed as reaction title*/
    // static GroupReactions(reactionDtos: ReactionDto[]): [GroupedReactions[], locationDict]{
    //     let groupedReactions : GroupedReactions[] = [];
    //     let positions : {[title: string]: number} = {};

    //     reactionDtos.forEach(r => {
    //         const currentItem = {[r.type] : r};
    //         if(positions && positions[r.type]){
    //             const id = positions[r.type];
    //             groupedReactions[id].reactions.push(r);
    //         }
    //         else{
    //             positions[r.type] = groupedReactions.length;
    //             groupedReactions.push({title: r.type, reactions: [r]});
    //         }
    //     })
    //     return [groupedReactions, positions];
    // }
}