namespace Tfs.Build.Paket

module Tasks =
    open Tfs.Build.Paket.GitHub
    open Tfs.Build.Paket.Utils
    open Paket

    let downloadLatestFromGitHub token destinationFileName logMessage logError =
        try
            System.IO.Path.GetDirectoryName(destinationFileName)
            |> ensureDir 

            let client = createClient token
            
            logMessage("fetching paket bootstrapper url")
            let latestBootstrapperUrl = 
                client |> 
                getBootstrapperUrl true  
                |> Async.RunSynchronously
            logMessage(sprintf "paket found at %s" latestBootstrapperUrl)
            
            downloadAndSave latestBootstrapperUrl destinationFileName 
            |> Async.RunSynchronously

            logMessage("installing paket.exe")
        with 
        | ex -> 
            logError(ex.ToString())

    let runBootstrapper file msg err =
        let logErr args = err(sprintf "%A" args)
        let logMsg args = msg(sprintf "%A" args)
        runexe file logErr logMsg
        |> (=) 0

    let downloadFileFromNetworkPath path destFile logFn errFn =
        ignore()
    
    let isNullOrEmpty (s :string) =
        s = null || s = ""
            
    type GetPaketTask() =
        inherit Microsoft.Build.Utilities.Task()
            
        override x.Execute () =
            let destinationFileName = System.IO.Path.Combine(x.Destination, "paket.bootstrapper.exe")

            match x.PathToPaketExe |> isNullOrEmpty, x.GitHubApiToken |> isNullOrEmpty  with
            | false, _ -> 
                downloadFileFromNetworkPath x.PathToPaketExe destinationFileName x.Log.LogMessage x.Log.LogError
                true
            | true, false ->
                downloadLatestFromGitHub x.GitHubApiToken destinationFileName x.Log.LogMessage x.Log.LogError
                runBootstrapper destinationFileName x.Log.LogMessage x.Log.LogError
            | true, true -> 
                x.Log.LogError("must have one of PathToPaketExe or GitHubApiToken")
                false
        
        [<Microsoft.Build.Framework.Required>]
        member val Destination = "" with get,set

        member val GitHubApiToken = "" with get,set
        member val PathToPaketExe = "" with get,set

    type RestoreTask() =
        inherit Microsoft.Build.Utilities.Task()

        override x.Execute () = 
            let depsFile = System.IO.Directory.GetFiles(x.SourceFolder, "paket.dependencies", System.IO.SearchOption.AllDirectories) |> Seq.firstOrDefault
            let referencesFiles = System.IO.Directory.GetFiles(x.SourceFolder, "paket.references", System.IO.SearchOption.AllDirectories) |> List.ofArray
            try 
                match depsFile, referencesFiles with
                | None, _ -> 
                    x.Log.LogError("no paket.dependencies file found. aborting restore.")
                    false
                | Some deps, refs ->
                    x.Log.LogMessage("found paket.dependencies and references files. restoring now")
                    Paket.RestoreProcess.Restore(deps, true, refs)
                    x.Log.LogMessage("restore complete")
                    true
            with
            | ex -> 
                x.Log.LogErrorFromException(ex, true)
                false

        [<Microsoft.Build.Framework.Required>]
        member val SourceFolder = "" with get,set