namespace Tfs.Build.Paket

module PaketHelpers =
    open Tfs.Build.Paket.Utils
    open Tfs.Build.Paket.GitHub

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

    let restoreFromSourceDir sourceDir logErrFn logMsgFn =
        if sourceDir |> (System.IO.Directory.Exists >> not) then 
            logErrFn "source directory not present"
            false
        else 
            let depsFile = getFilesRec sourceDir "paket.dependencies" |> Seq.firstOrDefault
            let referencesFiles = getFilesRec sourceDir "paket.references" |> List.ofArray
            try 
                match depsFile, referencesFiles with
                | None, _ -> 
                    logErrFn "no paket.dependencies file found. aborting restore."
                    false
                | Some deps, refs ->
                    logMsgFn "found paket.dependencies and references files. restoring now"
                    Paket.RestoreProcess.Restore(deps, true, refs)
                    logMsgFn "restore complete"
                    true
            with
            | ex -> 
                ex.ToString() |> logErrFn
                false

    let runBootstrapper file msg err =
        let logErr args = err(sprintf "%A" args)
        let logMsg args = msg(sprintf "%A" args)
        runexe file logErr logMsg
        |> (=) 0

