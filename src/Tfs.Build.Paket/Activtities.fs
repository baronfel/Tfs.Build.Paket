namespace Tfs.Build.Paket

type In<'a> = System.Activities.InArgument<'a>
type Out<'a> = System.Activities.OutArgument<'a>

module Activities =
    open Tfs.Build.Paket.GitHub
    open Tfs.Build.Paket.Utils
    open Tfs.Build.Paket.PaketHelpers
    open Microsoft.TeamFoundation.Build.Workflow.Activities

    let logMsg (context : System.Activities.CodeActivityContext) msg = context.TrackBuildMessage(msg, Microsoft.TeamFoundation.Build.Client.BuildMessageImportance.Normal)
    let logErr (context : System.Activities.CodeActivityContext) msg = context.TrackBuildError(msg)

    type PaketCallStatus = 
        | Successful = 0
        | Failed = 1

    type RestoreActivity() =
        inherit System.Activities.CodeActivity()

        override x.Execute context =
            let sourceFolder = context.GetValue x.SourceFolder 

            match restoreFromSourceDir sourceFolder (logErr context) (logMsg context) with
            | true -> context.SetValue(x.Status, PaketCallStatus.Successful)
            | false -> context.SetValue(x.Status, PaketCallStatus.Failed)
                 
        member val SourceFolder : In<string> = null with get,set
        member val Status : Out<PaketCallStatus> = null with get,set

    type AssertNoPrereleaseActivity() =
        inherit System.Activities.CodeActivity()

        override x.Execute context = ignore()
            // load deps file and make assertions about the results.
