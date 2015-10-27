# Tfs.Build.Paket
A set of helpful TFS Build Activities around getting and using [Paket](http://fsprojects.github.io/Paket/)


# Included Tasks
* `Tfs.Paket.Activities.RestoreActivity`
    * Given a path to the source code root, finds paket.dependencies and paket.references files and restores the packages therein.
    * takes a required parameter:
        * SourceFolder : the folder where your source code is
* `Tfs.Paket.Activities.AssertNoPrereleaseActivity`
    * Asserts that for a given paket.dependencies file there are no packages that are prerelease
    * takes a required parameter:
        * SourceFolder : the folder where your source code is
* `Tfs.Paket.Activities.AssertNoUnapprovedFeedsActivity`
    * Asserts that for a given paket.dependencies file there are no packages that come from an unapproved feed
    * takes three required parameters:
        * SourceFolder: the folder where your source code is
        * AllowedFeeds: a list of allowed package feed urls
        * ShouldError: should the presence of any of these feeds break the build
