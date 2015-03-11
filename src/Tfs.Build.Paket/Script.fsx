// Learn more about F# at http://fsharp.org. See the 'F# Tutorial' project
// for more guidance on F# programming.
#r @"..\packages\OctoKit\lib\net45\Octokit.dll"
#r @"..\packages\Microsoft.Net.Http\lib\net45\System.Net.Http.Extensions.dll"
#r @"..\packages\Microsoft.Net.Http\lib\net45\System.Net.Http.Primitives.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\System.Net.Http.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\Microsoft.Build.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\Microsoft.Build.Framework.dll"
#r @"C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\Microsoft.Build.Utilities.v4.0.dll"

#load "Utils.fs"
#load "GitHub.fs"
#load "Tasks.fs"

//let apiReleases = Paket.Tfs.GitHub.client.Release.GetAll("fsprojects", "paket") |> Async.AwaitTask |> Async.RunSynchronously
//let releases = Paket.Tfs.GitHub.releases () |> Async.RunSynchronously
//let latest = Paket.Tfs.GitHub.latestRelease true |> Async.RunSynchronously
//let assets = Paket.Tfs.GitHub.getBootstrapperUrl true |> Async.RunSynchronously
