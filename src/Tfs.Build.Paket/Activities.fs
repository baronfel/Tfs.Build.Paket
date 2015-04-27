namespace Tfs.Build.Paket

open Tfs.Build.Paket.PaketHelpers
open Microsoft.TeamFoundation.Build.Client
open System.Activities

type In<'a> = System.Activities.InArgument<'a>
type Out<'a> = System.Activities.OutArgument<'a>

module Activities =
    open Microsoft.TeamFoundation.Build.Workflow.Activities

    let logMsg (context : System.Activities.CodeActivityContext) msg = context.TrackBuildMessage(msg, BuildMessageImportance.High)
    let logErr (context : System.Activities.CodeActivityContext) msg = context.TrackBuildError(msg)

    let setResult (context : System.Activities.CodeActivityContext) (outProp : Out<'a>) trueVal falseVal input =
         match input with
         | true -> context.SetValue(outProp, trueVal)
         | false -> context.SetValue(outProp, falseVal) 

type PaketCallStatus = 
        | Successful = 0
        | Failed = 1

[<AbstractClassAttribute>]
type BaseActivity() =
    inherit CodeActivity()
    
    [<RequiredArgument>]
    member val SourceFolder : In<string> = null with get,set
    member val Status : Out<int> = null with get,set

[<BuildActivity(HostEnvironmentOption.Agent)>]
type RestoreActivity() =
    inherit BaseActivity()

    override x.Execute context =
        let sourceFolder = context.GetValue x.SourceFolder 

        restoreFromSourceDir sourceFolder (Activities.logErr context) (Activities.logMsg context)
        |> Activities.setResult context x.Status (int PaketCallStatus.Successful) (int PaketCallStatus.Failed)

[<BuildActivity(HostEnvironmentOption.Agent)>]
type AssertNoPrereleaseActivity() =
    inherit BaseActivity()

    override x.Execute context =
        let sourceFolder = context.GetValue x.SourceFolder

        hasPrereleases sourceFolder (Activities.logErr context) (Activities.logMsg context)
        |> not
        |> Activities.setResult context x.Status (int PaketCallStatus.Successful) (int PaketCallStatus.Failed)
    
[<BuildActivity(HostEnvironmentOption.Agent)>]
type AssertNoUnapprovedFeedsActivity() =
    inherit BaseActivity()

    [<RequiredArgument>]
    member val AllowedFeeds : In<ResizeArray<string>> = null with get,set
    [<RequiredArgument>]
    member val ShouldError : In<bool> = null with get,set

    override x.Execute context = 
        let sourceFolder = context.GetValue x.SourceFolder
        let feeds = context.GetValue x.AllowedFeeds |> List.ofSeq
        let failOnInvalids = context.GetValue x.ShouldError

        hasInvalidSources sourceFolder feeds failOnInvalids (Activities.logErr context) (Activities.logMsg context)
        |> not
        |> Activities.setResult context x.Status (int PaketCallStatus.Successful) (int PaketCallStatus.Failed)

