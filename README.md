# zapread.com
Website for zapread.com

The repository from https://github.com/Horndev/zapread.com-issues is being migrated to this one over time.  New issues should be created here, but legacy issues can still be found in the other issues github repository.

# Getting started with development

At this time, development is only confirmed working with Microsoft Windows (sorry).  It would be nice to port to .NET core, which can then run on any OS.  The reason it has not been ported yet is that .NET core was initially missing some basic Entity Framework features, but that has been resolved in newer releases.  The instructions here describe how to start development using a windows OS.

1.  Download Visual Studio Community 2017 (free).  https://visualstudio.microsoft.com/
1.  Clone the repository and all submodules
1.  Set up the AppSettings.config file (sensitive data not tracked in repository)
1.  Set up the database.config file (sensitive database connection data)
1.  Compile and deploy

## AppSettings

## database

Two SQL connection strings are required:

DefaultConnection: stores the user identity and log-in information

Zapread: stores the website content (accounts, posts, images, etc.)

# Contribution

[TODO]
