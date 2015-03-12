# Tfs.Build.Paket
A set of helpful TFS Build Activities around getting and using [Paket](http://fsprojects.github.io/Paket/)


# Included Tasks
* `Tfs.Paket.Tasks.RestoreActivity`
    * Given a path to the source code root, finds paket.dependencies and paket.references files and restores the packages therein.
    * takes a required parameter:
        * SourceFolder : the folder where your source code is

# Build and Deploy notes
This is using beta bits of F#, specifically for Seq.sortByDescending, so you'll need the latest VS 2015 CTP 6/F# compiler to use this.  The package will be updated to reference the beta version of F# on nuget in just a bit, so that deployments will be entirely self-contained.