namespace Tfs.Build.Paket

module PaketHelpers =
    open Tfs.Build.Paket.Utils

    let getDepsFile sourceDir =
        match getFilesRec sourceDir "paket.dependencies" with 
        | [] -> None
        | [x] -> Some x
        | _ -> None

    let getRefsFiles sourceDir =
        getFilesRec sourceDir "paket.references"
        
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
                | Some _, _->
                    logMsgFn "found paket.dependencies and references files. restoring now"
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
        | None -> []
        | Some lock -> 
            let resolution =
                lock.Groups
                |> Map.tryFind (Paket.Domain.GroupName "Main")
                |> Option.map (fun g -> g.Resolution |> Map.toList)
            defaultArg resolution []
             

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
        |> List.map (snd >> (fun pkg -> pkg.Source))
        |> List.choose (fun src -> match src with | Paket.PackageSources.NuGetV2 s -> Some s | _ -> None)
        |> List.map (fun nusrc -> nusrc.Url)
        |> Seq.ofList 
        |> Seq.distinct
        |> List.ofSeq

    let findOutliers (allowed : string list) (provided : string list) =
        provided |> List.filter (fun prov -> allowed |> List.exists (fun a -> prov.StartsWith(a)) |> not )

    let hasInvalidSources sourceDir (allowedSources : string list) failOnMatch logErrFn logMsgFn =

        sprintf "checking for the following sources: %A" allowedSources |> logMsgFn
                
        let distinctAllowed = allowedSources |> Seq.ofList |> Seq.distinct |> List.ofSeq
        let lockFileSources = sources sourceDir |> Seq.ofList |> Seq.map (fun s -> s.ToLowerInvariant()) |> Seq.distinct |> List.ofSeq
        
        let anyBadSources = findOutliers distinctAllowed lockFileSources

        let printBadSources srcs =
            srcs
            |> List.map (fun s -> sprintf "invalid package source: %s" s)
            |> String.concat System.Environment.NewLine

        match anyBadSources with
        | [] ->
            logMsgFn "no invalid sources found"
            false
        | srcs when not failOnMatch ->
            logMsgFn "found invalid package sources in the solution." 
            srcs
            |> printBadSources
            |> logMsgFn
            false
        | srcs -> 
            logErrFn "found invalid package sources in the solution." 
            srcs
            |> printBadSources
            |> logErrFn
            true

    let runBootstrapper file msg err =
        let logErr args = err(sprintf "%A" args)
        let logMsg args = msg(sprintf "%A" args)
        runexe file logErr logMsg
        |> (=) 0

