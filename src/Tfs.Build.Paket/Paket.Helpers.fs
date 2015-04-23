namespace Tfs.Build.Paket

module PaketHelpers =
    open Tfs.Build.Paket.Utils
    open Tfs.Build.Paket.GitHub

    let getDepsFile sourceDir =
        match getFilesRec sourceDir "paket.dependencies" with 
        | [] -> None
        | x::[] -> Some x
        | _ -> None

    let getRefsFiles sourceDir =
        getFilesRec sourceDir "paket.references"
        

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
            let depsFile = sourceDir |> getDepsFile
            let referencesFiles = sourceDir |> getRefsFiles
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

    let getLockFileDeps sourceDir =
        sourceDir 
        |> getDepsFile
        |> Option.map Paket.DependenciesFile.FindLockfile
        |> Option.map (fun fi -> Paket.LockFile.LoadFrom fi.FullName)

    let nugetPackages sourceDir =
        match getLockFileDeps sourceDir with
        | None -> List.empty
        | Some lock -> lock.ResolvedPackages |> Map.toList

    let hasPrereleases sourceDir logErrFn logMsgFn =
        match nugetPackages sourceDir |> List.filter (fun (_,p) -> p.Version.PreRelease.IsSome) with
        | [] -> 
            logMsgFn "No prereleases found"
            false
        | prereleases -> 
            prereleases
            |> List.map (fun (n,p) -> sprintf "%A - %A" n p.Version)
            |> String.concat (sprintf "%s" System.Environment.NewLine)
            |> sprintf "found packages that were prereleases:%s%s" System.Environment.NewLine
            |> logErrFn
            true

    let sources sourceDir =
        nugetPackages sourceDir
        |> List.map snd
        |> List.map (fun pkg -> pkg.Source)
        |> List.choose (fun src -> match src with | Paket.PackageSources.Nuget s -> Some s | _ -> None)
        |> List.map (fun nusrc -> nusrc.Url)
        |> Seq.ofList 
        |> Seq.distinct
        |> List.ofSeq

    let hasInvalidSources sourceDir (allowedSources : string list) failOnMatch logErrFn logMsgFn =
        let lockFileSources = sources sourceDir |> List.map (fun s -> s.ToLowerInvariant())
        let invalids = 
            lockFileSources |> Set.ofList
            |> Set.union (allowedSources |> List.map (fun s -> s.ToLowerInvariant()) |> Set.ofList)
        match invalids.IsEmpty with
        | true | false when not failOnMatch ->
            logMsgFn "no invalid sources found"
            false
        | _ -> 
            logErrFn "found invalid package sources in the solution." 
            invalids
            |> Set.map (fun s -> sprintf "invalid package source: %s" s)
            |> Set.iter logErrFn
            true

    let runBootstrapper file msg err =
        let logErr args = err(sprintf "%A" args)
        let logMsg args = msg(sprintf "%A" args)
        runexe file logErr logMsg
        |> (=) 0

