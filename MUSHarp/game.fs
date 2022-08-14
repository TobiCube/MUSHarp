namespace MUSHarp
open  FSharp.Collections


type CommandAttribute() = inherit System.Attribute()


module Game =

  type GameObject() =
    //Available without needing to pass lock
    member val getLock = AnyLock with get, set
    member val setLock = AnyLock with get, set
    member val name = "<GameObject>" with get, set
    member val desc = "<Desc>" with get, set
    
    //Hidden without the ability to pass lock
    member val id = 0 with get, set //TODO: Randomize this number and check
                               //To ensure that no other objects have it
                               //Also maybe make it immutable after initializate
    

  and PlayerObject() =
    inherit GameObject()
    member val Approved = false with get, set
    member val Admin = false with get, set
  and GroupObject(P : PlayerObject) =
    inherit GameObject()
    member val joinLock = AnyLock with get, set
    member val viewMembersLock = AnyLock with get, set //TODO: Always ensure members
                                                   //Of the group can view membership
    member val kickLock = AdminLock with get, set //TODO: Always ensure you can leave
                                              //a group you're part of.

    //TODO: Actually lock this.
    member val MemberList = Map[(P.id,P)] with get, set


  and Lock =
      | AnyLock                 //This lock is passed by everyone
      | NoLoginLock             //This lock is only passed if you are not logged in
      | AdminLock               //This lock is only passed if you are an admin
      | ObjectLock of GameObject//You must be this game object to pass this lock
      | GroupLock of GroupObject//You must be part of this group
      | OrLock of Lock * Lock   //Either lock can be passed
      | AndLock of Lock * Lock  //Both must be passed

      [<Command>]
      member this.Auth(O : GameObject) =
        match this with
          | AnyLock -> true
          | NoLoginLock -> false
          | AdminLock -> (match O with
                                    | :? PlayerObject as player -> player.Admin
                                    | _ -> false)
          | ObjectLock(o) -> O.id = o.id
          | GroupLock(g) -> g.MemberList.ContainsKey(O.id)
          | OrLock(l,r) -> l.Auth(O) || r.Auth(O)
          | AndLock(l,r) -> l.Auth(O) && r.Auth(O)