namespace MUSHarp

module Game2 = 

    open System
    open System.Text.Json.Nodes
    open System.Collections.Generic
    //A method for locking access to objects
    //A class for providing the locks for the locking method
    //A class to provide the infrastructure for JSON serialization
    //A class to provide groups, players, and object wrappers for locks and rooms

    type IJsonObject = 
        ///<summary> 
        /// Generate a JsonObject from the properties and fields of the object
        ///</summary>
        abstract GetJsonObject: unit -> JsonNode
        ///<summary>
        /// Create a new object with data from a JsonObject that represents it
        ///</summary>
        abstract InitializeFromJsonObject: JsonNode -> System.Object

    type LockFailureException() = inherit System.Exception()
    type MismatchedJsonException() = inherit System.Exception()

    type Lock =
        | AnyLock                               //This lock is passed by everyone
        | NoLoginLock                           //This lock is only passed if you are not logged in
        | AdminLock                             //This lock is only passed if you are an admin
        | SelfLock of GameObject                //You must be this game object to pass this lock
        | ContainsLock of GameObject            //The object must be contained within to pass the lock
        | ParentLock of GameObject              //The object must be have a specific parent to pass
        | OrLock of Lock * Lock                 //Either lock can be passed
        | AndLock of Lock * Lock                //Both must be passed
        | NotLock of Lock                       //You must not pass the given lock to unlock

        member this.auth(O : GameObject) =
            let mutable Override = PlayerObject()
            Override.isAdmin <- true
            match this with
            | _ when O :? PlayerObject && (O:?>PlayerObject).isAdmin -> true
            | AdminLock ->  false
            | AnyLock -> true
            | NoLoginLock -> false
            | SelfLock(o) -> O.get_id(Override) = o.get_id(Override)
            | ContainsLock(c) -> Map.containsKey (O.get_id(Override)) c.children
            | ParentLock(p) -> Map.containsKey (O.get_id(Override)) p.parents
            | OrLock(l,r) -> l.auth(O) || r.auth(O)
            | AndLock(l,r) -> l.auth(O) && r.auth(O)
            | NotLock(l) -> not (l.auth(O))
    
        static member read (lock: Lock) (a: 'a) (obj: GameObject): 'a= 
            if lock.auth(obj) then a
            else raise (LockFailureException())
        static member write (lock: Lock) (a: 'a) (obj: GameObject) (b: 'a)= 
            if lock.auth(obj) then 
                let mutable c = a
                c <- b 
                c 
            else raise (LockFailureException())

        interface IJsonObject with
            member this.GetJsonObject() = 
                let mutable Override = PlayerObject()
                Override.isAdmin <- true
                match this with 
                    | AnyLock                       -> JsonValue.Create("AnyLock")
                    | NoLoginLock                   -> JsonValue.Create("NoLoginLock")
                    | AdminLock                     -> JsonValue.Create("AdminLock")
                    | SelfLock(o)       -> JsonObject([|KeyValuePair<string,JsonNode>("SelfLock",o.get_id(Override))|])
                    | ContainsLock(o)   -> JsonObject([|KeyValuePair<string,JsonNode>("ContainsLock",o.get_id(Override))|])
                    | ParentLock(o)     -> JsonObject([|KeyValuePair<string,JsonNode>("ParentLock",o.get_id(Override))|])
                    | OrLock(l,r)       -> JsonObject([KeyValuePair<string,JsonNode>("OrLockLeft",(l:>IJsonObject).GetJsonObject());KeyValuePair<string,JsonNode>("OrLockRight",(r:>IJsonObject).GetJsonObject())])
                    | AndLock(l,r)      -> JsonObject([KeyValuePair<string,JsonNode>("AndLockLeft",(l:>IJsonObject).GetJsonObject());KeyValuePair<string,JsonNode>("AndLockRight",(r:>IJsonObject).GetJsonObject())])
                    | NotLock(l)                -> JsonObject([|KeyValuePair<string,JsonNode>("NotLock",(l:>IJsonObject).GetJsonObject())|])
            member this.InitializeFromJsonObject(obj: JsonNode) =
                let lock = AnyLock
                match obj with 
                    | :? JsonValue  when obj.GetValue() = "AnyLock"     -> AnyLock
                    | :? JsonValue  when obj.GetValue() = "NoLoginLock" -> NoLoginLock
                    | :? JsonValue  when obj.GetValue() = "AdminLock"   -> AdminLock
                    | :? JsonObject when (obj:?>JsonObject).ContainsKey("SelfLock")    -> SelfLock (GameObject.Fetch(obj["SelfLock"].GetValue()))
                    | :? JsonObject when (obj:?>JsonObject).ContainsKey("ContainsLock")    -> ContainsLock (GameObject.Fetch(obj["ContainsLock"].GetValue()))
                    | :? JsonObject when (obj:?>JsonObject).ContainsKey("ParentLock")    -> ParentLock (GameObject.Fetch(obj["ParentLock"].GetValue()))
                    | :? JsonObject when (obj:?>JsonObject).ContainsKey("OrLockLeft")    -> OrLock ((lock:>IJsonObject).InitializeFromJsonObject(obj["OrLockLeft"]):?>Lock, (AnyLock:>IJsonObject).InitializeFromJsonObject(obj["OrLockRight"]):?>Lock)
                    | :? JsonObject when (obj:?>JsonObject).ContainsKey("AndLockLeft")    -> AndLock ((lock:>IJsonObject).InitializeFromJsonObject(obj["AndLockLeft"]):?>Lock, (AnyLock:>IJsonObject).InitializeFromJsonObject(obj["AndLockRight"]):?>Lock)
                    | :? JsonObject when (obj:?>JsonObject).ContainsKey("NotLock")    -> NotLock ((lock:>IJsonObject).InitializeFromJsonObject(obj["NotLock"]):?>Lock)
                    | _ -> (raise (MismatchedJsonException()))
                    
    and GameObject() = 
        let mutable _id = -1
        member val id_lock = AnyLock with get, set
        member val name = "<GameObject>" with get, set
        member val desc = "<Desc>" with get, set
        member val parents: Map<int,GameObject>  = Map.empty<int,GameObject> with get, set
        member val children: Map<int,GameObject> = Map.empty<int,GameObject> with get, set

        member this.set_id obj id = _id <- Lock.write this.id_lock _id obj id
        member this.get_id = Lock.read this.id_lock _id

        //TODO: Make this lazy? Also make it to begin with
        static member Fetch(id: int) = 
            GameObject()
        
    and PlayerObject() = 
        inherit GameObject()
        member val isAdmin = false with get, set
    
    type RoomObject() = inherit GameObject()
    type ChannelObject() = inherit GameObject()
    type PermissionObject() = inherit GameObject()
    type GroupObject() = inherit GameObject()

