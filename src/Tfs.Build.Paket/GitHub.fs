namespace Tfs.Build.Paket

module GitHub =
    open Octokit
    
    let createClient token = 
        let creds = Octokit.Credentials(token)
        let credStore = { new Octokit.ICredentialStore with member x.GetCredentials() = System.Threading.Tasks.Task.FromResult(creds) }
        GitHubClient(ProductHeaderValue("TFSPaketTasks"), credStore)
        
    let releases (client : GitHubClient) =
        async {
            let! releases = client.Release.GetAll("fsprojects", "paket") |> Async.AwaitTask
            return releases  :> seq<Release>
        }

    let private isStable (r: Octokit.Release) = 
        not r.Prerelease

    let latestRelease stable client =
        async {
            let! releases = releases client
            return releases
            |> Seq.toList
            |> List.filter (fun r -> stable && isStable r)
            |> List.sortBy (fun r -> r.CreatedAt)
            |> List.rev
            |> List.head
        }
    
    let private asset releaseid name (client : GitHubClient) =
        async {
            let! assets = client.Release.GetAssets("fsprojects", "paket", releaseid) |> Async.AwaitTask
            return assets
            |> Seq.find (fun a -> a.Name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
        }

    let getBootstrapperUrl stable client = 
        async {
            let! latest = client |> latestRelease stable
            let! latestBootstrapper = client |> asset latest.Id "paket.bootstrapper.exe" 
            return latestBootstrapper.BrowserDownloadUrl
        }