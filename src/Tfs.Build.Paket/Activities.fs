namespace Tfs.Build.Paket

open Tfs.Build.Paket.PaketHelpers
open Microsoft.TeamFoundation.Build.Client;

type In<'a> = System.Activities.InArgument<'a>
type Out<'a> = System.Activities.OutArgument<'a>

module Activities =
    open System.Collections.Generic
    open Tfs.Build.Paket.GitHub
    open Tfs.Build.Paket.Utils
    open Tfs.Build.Paket.PaketHelpers
    open Microsoft.TeamFoundation.Build.Workflow.Activities

    let logMsg (context : System.Activities.CodeActivityContext) msg = context.TrackBuildMessage(msg, Microsoft.TeamFoundation.Build.Client.BuildMessageImportance.Normal)
    let logErr (context : System.Activities.CodeActivityContext) msg = context.TrackBuildError(msg)
    let setResult (context : System.Activities.CodeActivityContext) (outProp : Out<'a>) trueVal falseVal input =
         match input with
         | true -> context.SetValue(outProp, trueVal)
         | false -> context.SetValue(outProp, falseVal) 

type PaketCallStatus = 
        | Successful = 0
        | Failed = 1

[<BuildActivityAttribute(HostEnvironmentOption.Agent)>]
type RestoreActivity() =
    inherit System.Activities.CodeActivity()

    member val SourceFolder : In<string> = null with get,set
    member val Status : Out<int> = null with get,set

    override x.Execute context =
        let sourceFolder = context.GetValue x.SourceFolder 

        restoreFromSourceDir sourceFolder (Activities.logErr context) (Activities.logMsg context)
        |> Activities.setResult context x.Status (int PaketCallStatus.Successful) (int PaketCallStatus.Failed)

[<BuildActivityAttribute(HostEnvironmentOption.Agent)>]
type AssertNoPrereleaseActivity() =
    inherit System.Activities.CodeActivity()

    member val SourceFolder : In<string> = null with get,set
    member val Status : Out<int> = null with get,set

    override x.Execute context =
        let sourceFolder = context.GetValue x.SourceFolder
        hasPrereleases sourceFolder (Activities.logErr context) (Activities.logMsg context)
        |> Activities.setResult context x.Status (int PaketCallStatus.Successful) (int PaketCallStatus.Failed)
    
[<BuildActivityAttribute(HostEnvironmentOption.Agent)>]
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

        hasInvalidSources sourceFolder feeds failOnInvalids (Activities.logErr context) (Activities.logMsg context)
        |> not
        |> Activities.setResult context x.Status (int PaketCallStatus.Successful) (int PaketCallStatus.Failed)

