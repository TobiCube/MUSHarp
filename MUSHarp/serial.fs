namespace MUSHarp

module serial = 
    open System
    open System.Text.Json.Nodes
    open System.Collections.Generic

    type JAttribute() = inherit Attribute()

    type IJsonSerializable = 
        [<J>]
        abstract GetJsonObject : unit -> (int * string * KeyValuePair<string,JsonNode> array)
        [<J>]
        abstract ConvertJsonObject : (int *string * KeyValuePair<string,JsonNode> array) -> Object


    type Cube(id:int, name:string, cube:int, test:int) =
        member val id = id with get
        member val name = name with get, set
        member val cube = cube with get, set
        member val test = test with get, set

        new() = Cube(-1,"invalid",5,6)

        override this.ToString() =
            "MUSHarp.World.Cube: {id: "+id.ToString()+", name: "+name+", cube: "+cube.ToString()+", test: "+test.ToString()+"}"
        interface IJsonSerializable with
            member this.GetJsonObject() = 
                (this.id, 
                this.name, 
                [|KeyValuePair("cube",JsonValue.Create(this.cube)); 
                KeyValuePair("test",JsonValue.Create(this.test)) |])
            member this.ConvertJsonObject((id: int, name: string, prop: KeyValuePair<string,JsonNode> array)) = 
                    let dict = Dictionary(prop)
                    let cube: int = dict["cube"].GetValue()
                    let test: int = dict["test"].GetValue()
                    Cube(id, name, cube, test)

    
    do 

        let a = Cube(1,":D",2,3)
        let a_obj = (a :> IJsonSerializable).GetJsonObject()
        let b = Cube()
        let c = (b :> IJsonSerializable).ConvertJsonObject(a_obj) :?> Cube
        Console.WriteLine(a)
        Console.WriteLine(b)
        Console.WriteLine(c)

        
        Console.WriteLine("Please press any button to exit")
        Console.ReadKey() |> ignore