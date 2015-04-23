namespace Tfs.Build.Paket

type In<'a> = System.Activities.InArgument<'a>
type Out<'a> = System.Activities.OutArgument<'a>

module Activities =
    open System.Collections.Generic
    open Tfs.Build.Paket.GitHub
    open Tfs.Build.Paket.Utils
    open Tfs.Build.Paket.PaketHelpers
    open Microsoft.TeamFoundation.Build.Workflow.Activities

    type PaketCallStatus = 
        | Successful = 0
        | Failed = 1

    let logMsg (context : System.Activities.CodeActivityContext) msg = context.TrackBuildMessage(msg, Microsoft.TeamFoundation.Build.Client.BuildMessageImportance.Normal)
    let logErr (context : System.Activities.CodeActivityContext) msg = context.TrackBuildError(msg)
    let setResult (context : System.Activities.CodeActivityContext) (outProp : Out<'a>) trueVal falseVal input =
         match input with
         | true -> context.SetValue(outProp, trueVal)
         | false -> context.SetValue(outProp, falseVal) 

    type RestoreActivity() =
        inherit System.Activities.CodeActivity()

        member val SourceFolder : In<string> = null with get,set
        member val Status : Out<int> = null with get,set

        override x.Execute context =
            let sourceFolder = context.GetValue x.SourceFolder 

            restoreFromSourceDir sourceFolder (logErr context) (logMsg context)
            |> setResult context x.Status (int PaketCallStatus.Successful) (int PaketCallStatus.Failed)

    type AssertNoPrereleaseActivity() =
        inherit System.Activities.CodeActivity()

        member val SourceFolder : In<string> = null with get,set
        member val Status : Out<int> = null with get,set

        override x.Execute context =
            let sourceFolder = context.GetValue x.SourceFolder
            hasPrereleases sourceFolder (logErr context) (logMsg context)
            |> setResult context x.Status (int PaketCallStatus.Successful) (int PaketCallStatus.Failed)
    
    type AssertNoUnapprovedFeedsActivity() =
        inherit System.Activities.CodeActivity()

        member val SourceFolder : In<string> = null with get,set
        member val Status : Out<int> = null with get,set

        member val AllowedFeeds : In<ResizeArray<string>> = null with get,set
        member val ShouldError : In<bool> = null with get,set

        override x.Execute context = 
            let sourceFolder = context.GetValue x.SourceFolder
            let feeds = context.GetValue x.AllowedFeeds |> List.ofSeq
            let failOnInvalids = context.GetValue x.ShouldError

            hasInvalidSources sourceFolder feeds failOnInvalids (logErr context) (logMsg context)
            |> not
            |> setResult context x.Status (int PaketCallStatus.Successful) (int PaketCallStatus.Failed)

