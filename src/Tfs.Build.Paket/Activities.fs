﻿namespace Tfs.Build.Paket

type In<'a> = System.Activities.InArgument<'a>
type Out<'a> = System.Activities.OutArgument<'a>

module Activities =
    open Tfs.Build.Paket.GitHub
    open Tfs.Build.Paket.Utils
    open Tfs.Build.Paket.PaketHelpers
    open Microsoft.TeamFoundation.Build.Workflow.Activities

    type PaketCallStatus = 
        | Successful = 0
        | Failed = 1

    let logMsg (context : System.Activities.CodeActivityContext) msg = context.TrackBuildMessage(msg, Microsoft.TeamFoundation.Build.Client.BuildMessageImportance.Normal)
    let logErr (context : System.Activities.CodeActivityContext) msg = context.TrackBuildError(msg)
    let setResult (context : System.Activities.CodeActivityContext) (outProp : Out<PaketCallStatus>) input =
         match input with
         | true -> context.SetValue(outProp, PaketCallStatus.Successful)
         | false -> context.SetValue(outProp, PaketCallStatus.Failed) 

    [<AbstractClass>]
    type PaketActivityBase() =
        inherit System.Activities.CodeActivity()

        member val SourceFolder : In<string> = null with get,set
        member val Status : Out<PaketCallStatus> = null with get,set

    type RestoreActivity() =
        inherit PaketActivityBase()

        override x.Execute context =
            let sourceFolder = context.GetValue x.SourceFolder 

            restoreFromSourceDir sourceFolder (logErr context) (logMsg context)
            |> setResult context x.Status

    type AssertNoPrereleaseActivity() =
        inherit PaketActivityBase()

        override x.Execute context =
            let sourceFolder = context.GetValue x.SourceFolder
            hasPrereleases sourceFolder (logErr context) (logMsg context)
            |> setResult context x.Status