namespace Tfs.Build.Paket

module Utils =
    let downloadAndSave (uri : string) dest =
        async {
            use client = new System.Net.Http.HttpClient()
            let! downloadStream = client.GetStreamAsync(uri) |> Async.AwaitTask
            use filestream = new System.IO.FileStream(dest, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.None)
            return! downloadStream.CopyToAsync(filestream) |> Async.AwaitTask
        }
    let ensureDir dir =
        if System.IO.Directory.Exists(dir) |> not 
        then System.IO.Directory.CreateDirectory(dir) |> ignore        
     
    let runexe exeName errFunc outputFunc =
        let mutable processInfo = new System.Diagnostics.ProcessStartInfo(exeName)
        processInfo.CreateNoWindow <- true
        processInfo.RedirectStandardError <- true
        processInfo.RedirectStandardOutput <- true
        processInfo.UseShellExecute <- false
        let mutable proc = new System.Diagnostics.Process()
        proc.StartInfo <- processInfo
        proc.ErrorDataReceived.Add(errFunc)   
        proc.OutputDataReceived.Add(outputFunc)
        proc.Start() |> ignore
        proc.WaitForExit()        
        proc.ExitCode