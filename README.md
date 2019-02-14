# ZapRead.com

[![Build status](https://horndev.visualstudio.com/Coinpanic/_apis/build/status/ZapRead-ASP.NET-CI)](https://horndev.visualstudio.com/Coinpanic/_build/latest?definitionId=2)
[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2FHorndev%2Fzapread.com.svg?type=shield)](https://app.fossa.io/projects/git%2Bgithub.com%2FHorndev%2Fzapread.com?ref=badge_shield)
[![BCH compliance](https://bettercodehub.com/edge/badge/Horndev/zapread.com?branch=master)](https://bettercodehub.com/)
[![CodeFactor](https://www.codefactor.io/repository/github/horndev/zapread.com/badge/master)](https://www.codefactor.io/repository/github/horndev/zapread.com/overview/master)

Website for zapread.com

Current version is: 0.2-beta.  You can track progress to the next version on the [projects page](https://github.com/Horndev/zapread.com/projects).

## ZapRead.com Mission

1) To create a social economy.  To turn social media and networking upside-down.  To give users and the community control over their content.  Stop the selling of user data and innunduation of advertising without compensation.

2) To create a new publishing model.  Previously - authors paid publishers who then charged users - this is not reflective of the value of publishers, as both consumers and authors were at the mercy of the publisher.  *Authors should be properly compensated for their work - by the value as determined by the consumers.*

### Vote Examples

100 satoshi up vote:

* 60 goes to author
* 20 goes to group
* 10 goes to community
* 10 goes to zapread

100 satoshi down vote:

* 0 goes to author
* 80 goes to group
* 10 goes to community
* 10 goes to zapread

The voting affects the post score. 

### Group payments

When funds are sent to a group, not all posts in the group receive a portion.

Factors: 

* Time since post was made
* Post score
* receiving funds is a lottery (minimum 1 satoshi received)

We don't want groups to be small, and there should be a incentive for continuing to post to bigger groups (more posts already)

Example:

Group A, 20,000 posts, first post 1 year ago

100 satoshi vote

Each post has a probability to receive funds.  Older posts have the least likelihood.

Variable 1:  Post half life.  (Lambda)

Lambda = 30 days.

Post score means that higher posts are more likely to win funds.

Variable 2:  Vote max divisions (M)

Variable 3:  Vote min division (1 satoshi)

This is the maximum number of draws which are made for a distribution to a group.  So if M = 1000, then a 10,000 satoshi vote will have 1000 lotteries.

So, the algorithm is to pull M winners, biased by Lambda.

### User Reputation

When logged-in users vote a post up or down, it modifies the authors reputation proportional to the amount paid.

Users with high reputation need to spend less to move the post score, while users with low reputation need to spend more.  This is an incentive to create content which increases user reputation.  Spam and unwelcome posts will be inhibited as there is no reward for such behaviour.

# Getting started with development

At this time, development is only confirmed working with Microsoft Windows (sorry).  It would be nice to port to .NET core, which can then run on any OS.  The reason it has not been ported yet is that .NET core was initially missing some basic Entity Framework features, but that has been resolved in newer releases.  The instructions here describe how to start development using a windows OS.

1.  Download Visual Studio Community 2017 (free).  https://visualstudio.microsoft.com/
1.  Clone the repository and all submodules
1.  Set up the AppSettings.config file (sensitive data not tracked in repository)
1.  Set up the database.config file (sensitive database connection data)
1.  Compile and deploy

## AppSettings.config

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

## database.config

Two SQL connection strings are required:

DefaultConnection: stores the user identity and log-in information

Zapread: stores the website content (accounts, posts, images, etc.)

# Contribution

Contribution to this repository is done through pull requests.  

By contributing to this project, you are giving permission to the copyright owner to change the software license without notice.  Any changes to the license will only apply to versions of the software after the license has changed.

## License

[![FOSSA Status](https://app.fossa.io/api/projects/git%2Bgithub.com%2FHorndev%2Fzapread.com.svg?type=large)](https://app.fossa.io/projects/git%2Bgithub.com%2FHorndev%2Fzapread.com?ref=badge_large)
