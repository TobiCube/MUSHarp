namespace MUSHarp
open MUSHarp.Game2
open System

module World = 

    let mutable public Objects: Map<int,GameObject> = Map.empty<int,GameObject>



    [<EntryPoint>]
    do 
        let mutable adminobj = PlayerObject() 
        adminobj.isAdmin <- true
        adminobj.set_id adminobj 1
        let gameobj = GameObject()
        gameobj.set_id adminobj 21
        let gameobj2 = GameObject()
        gameobj.set_id adminobj 22
        let playerobj = PlayerObject()
        playerobj.set_id adminobj 30

        Objects <- Map[|(adminobj.get_id(adminobj),adminobj);(gameobj.get_id(adminobj),gameobj);(gameobj2.get_id(adminobj),gameobj2);(playerobj.get_id(adminobj),playerobj)|]
        

        let lock = AndLock (AnyLock, OrLock (AdminLock, OrLock (SelfLock(playerobj),SelfLock(gameobj2))))
        Console.WriteLine(lock.auth(adminobj))
        Console.WriteLine(lock.auth(gameobj))
        Console.WriteLine(lock.auth(gameobj2))
        Console.WriteLine(lock.auth(playerobj))
        let json = (lock:>IJsonObject).GetJsonObject()
        Console.WriteLine(json.ToJsonString())
        let o = (AnyLock:>IJsonObject).InitializeFromJsonObject(json) :?> Lock
        let json2 = (o:>IJsonObject).GetJsonObject()
        Console.WriteLine(o.auth(adminobj))
        Console.WriteLine(o.auth(gameobj))
        Console.WriteLine(o.auth(gameobj2))
        Console.WriteLine(o.auth(playerobj))
        Console.WriteLine(json2.ToJsonString())
        Console.ReadKey() |> ignore
