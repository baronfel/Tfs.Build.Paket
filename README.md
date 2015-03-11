# Tfs.Build.Paket
A set of helpful TFS Build tasks around getting and using [Paket](http://fsprojects.github.io/Paket/)


# Included Tasks
* `Tfs.Paket.Tasks.GetPaketTask`
    * Fetches the latest stable version of Paket from either a network location or GitHub releases
    * takes two required parameters:
        * Destination : the folder where paket.bootstrapper.exe and paket.exe will be downloaded to,
        * And one of either
            * PathToPaketExe : the network folder where Paket.exe lives
            * GitHubApiToken : your [token](https://github.com/blog/1509-personal-api-tokens) for querying and downloading via the GitHub API.
