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
* `Tfs.Paket.Tasks.RestoreTask`
    * Given a path to the source code root, finds paket.dependencies and paket.references files and restores the packages therein.
    * takes a required parameters:
        * SourceFolder : the folder where your source code is

# Build and Deploy notes
This is using beta bits of F#, specifically for Seq.sortByDescending, so you'll need the latest VS 2015 CTP 6/F# compiler to use this.  The package will be updated to reference the beta version of F# on nuget in just a bit, so that deployments will be entirely self-contained.