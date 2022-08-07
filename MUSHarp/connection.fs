namespace MUSHarp

module Connector =

    open Game
    open System.Threading
    open System.Threading.Tasks
    open System.Net.Sockets

    type Connection (client : TcpClient) =
      let netstream = client.GetStream()
      let encode(str : string) =
        System.Text.Encoding.ASCII.GetBytes(str)
      let decode(bytes : byte[]) =
        System.Text.Encoding.ASCII.GetString(bytes)
      let send(str : System.Object) =
          let bytes = encode(str :?> string)
          netstream.Write(bytes,0,bytes.Length);
      let parse(str : string) =
        let str2 = str.Split(' ',System.StringSplitOptions.RemoveEmptyEntries)
        let str3 = str2.[0].Split('/',System.StringSplitOptions.RemoveEmptyEntries)
        let cmd = str3.[0]
        let switches = Array.sub str3 1 (str3.Length - 1)
        let args = Array.sub str2 1 (str2.Length - 1)

        System.Console.WriteLine(str);
        System.Console.WriteLine(cmd);
        for s in switches do
          System.Console.WriteLine("switch: "+s)
        for s in args do
          System.Console.WriteLine("arg: "+s)

      let receive() =
        while true do
          while client.Available = 0 do Thread.Sleep(100)
          let bytes = Array.create client.Available System.Byte.MinValue
          netstream.Read(bytes,0,bytes.Length) |> ignore
          parse(decode(bytes))
      let receive_task = Task.Run(receive)

      member this.SendData with set(value : string) =
          do
            let send_action = System.Action<System.Object>(send)
            let send_task = new Task(send_action,value)
            send_task.Start()
    open System
    open System.Net

    [<EntryPoint>]
    do
        Console.WriteLine("Starting Server...")
        try
            let server = TcpListener(IPAddress.Loopback,55555)
            server.Start()
            Console.WriteLine("Startup Successful")
            while true do
              Console.WriteLine("Waiting for clients...")
              while not(server.Pending()) do Thread.Sleep(100) //Wait till there's a client
              Console.Write("Connection request...")
              let client = server.AcceptTcpClient()
              let Conn = Connection(client)
              Conn.SendData <- "Welcome cube"
              Console.WriteLine("accepted")
        with
          | e -> Console.WriteLine("Exception caught: "+e.ToString()+"\n"+e.StackTrace)