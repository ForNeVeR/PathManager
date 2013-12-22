module Wmi

open FSharp.Management

type LocalWmiProvider = WmiProvider<"localhost">

let IsUpdateInstalled (hotFixId : string) : bool =
    let data = LocalWmiProvider.GetDataContext()
    data.Win32_QuickFixEngineering
    |> Seq.exists (fun d -> d.HotFixID = hotFixId)
