# ZapRead.com

[![Build status](https://horndev.visualstudio.com/Coinpanic/_apis/build/status/ZapRead-ASP.NET-CI)](https://horndev.visualstudio.com/Coinpanic/_build/latest?definitionId=2)

Website for zapread.com

The repository from https://github.com/Horndev/zapread.com-issues is being migrated to this one over time.  New issues should be created here, but legacy issues can still be found in the other issues github repository.

Current version is: 0.1-beta

# Getting started with development

At this time, development is only confirmed working with Microsoft Windows (sorry).  It would be nice to port to .NET core, which can then run on any OS.  The reason it has not been ported yet is that .NET core was initially missing some basic Entity Framework features, but that has been resolved in newer releases.  The instructions here describe how to start development using a windows OS.

1.  Download Visual Studio Community 2017 (free).  https://visualstudio.microsoft.com/
1.  Clone the repository and all submodules
1.  Set up the AppSettings.config file (sensitive data not tracked in repository)
1.  Set up the database.config file (sensitive database connection data)
1.  Compile and deploy

## AppSettings

The AppSettings.config file should contain the following keys:

```
<?xml version="1.0"?>
<appSettings>
  <add key="webpages:Version" value="3.0.0.0" />
  <add key="webpages:Enabled" value="false" />
  <add key="ClientValidationEnabled" value="true" />
  <add key="UnobtrusiveJavaScriptEnabled" value="true" />

  <add key="OAuth_Google_ClientId" value="[INSERT HERE]"/>
  <add key="OAuth_Google_Secret" value="[INSERT HERE]"/>

  <add key="OAuth_Reddit_ClientId" value="[INSERT HERE]"/>
  <add key="OAuth_Reddit_Secret" value="[INSERT HERE]"/>
  
  <!-- Lightning LND backend only -->
  <add key="LnUseTestnet" value="false"/> 
  
  <add key="LnMainnetHost" value="[INSERT HERE]"/>
  <add key="LnTestnetHost" value="127.0.0.1"/>
  <add key="LnPubkey" value="[INSERT HERE]"/>

  <add key="LnTestnetMacaroonInvoice" value="[INSERT HERE]"/>
  <add key="LnTestnetMacaroonRead" value="[INSERT HERE]"/>
  <add key="LnTestnetMacaroonAdmin" value="[INSERT HERE]"/>
  <add key="LnMainnetMacaroonInvoice" value="[INSERT HERE]"/>
  <add key="LnMainnetMacaroonRead" value="[INSERT HERE]"/>
  <add key="LnMainnetMacaroonAdmin" value="[INSERT HERE]"/>

</appSettings>
```

## Database

Two SQL connection strings are required:

DefaultConnection: stores the user identity and log-in information

Zapread: stores the website content (accounts, posts, images, etc.)

# Contribution

[TODO]
