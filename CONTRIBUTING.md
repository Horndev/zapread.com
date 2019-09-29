# Contribution Guide

Thank you for taking interest in the Zapraed.com community!  We welcome your contributions, ideas, and help.
This guide lays out a few simple principals that have been adopted to help us all work together.

## Bug reports

If you identify an problem, please report it using the github issue tracker.

## Setting up local development

1.  Download Visual Studio Community 2017 or 2019 (free).  https://visualstudio.microsoft.com/
1.  Clone the repository and all submodules
1.  Set up the AppSettings.config file (copy AppSettings.config.template to AppSettings.config)
1.  Set up the database.config file (copy database.config.template to database.config)
1.  Open zapread.com.sln with Visual Studio
1.  In the Package Manager Console, run `Update-Package Microsoft.CodeDom.Providers.DotNetCompilerPlatform -r` to set up compiler
1.  Run zapread.com (e.g. press F5)
1.  Go to /Home/Install/ to bootstrap database

### Creating your admin account and security

1.	Set the AppSettings.config key `AdminMasterPassword` to any private value you wish.
1.  Navigate to /Home/Install/
1.  Enter the key in the page, and set the user you wish to grant the site administration role to.
1.  Click Grant.
1.  Log off and back on to enable site administration.
1.  Change the AppSettings.config key `EnableInstall` to `false` for added security.

## Contributing your changes

Submit a github pull request to the dev branch.

## Documentation

Please help!  If you're reading through the code and struggle to understand something, and later learn what is going on - add some documentation so that the next person doesn't struggle.
It is likely there is something complex in that part of the code that others may appreciate assitance with in documentation.

## Coding conventions

Standard C# coding conventions are used as much as possible.

### Style

LINQ queries are implemented using lambda functions and not in-line language.

## Thank you!

Your contributions help make Zapread.com better for everyone.  
Share your contributions with a post on Zapread and hopefully the community will reward you with votes.

