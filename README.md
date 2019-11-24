# NFLBot

[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fd2tools%2FNFLBot.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2FKiaArmani%2FNFLBot?ref=badge_shield)
[![Build Status](https://travis-ci.org/d2tools/NFLBot.svg?branch=master)](https://travis-ci.org/d2tools/NFLBot)

A Discord Bot that collects Nightfall Activities of your Destiny Clan and providing commands for scoreboards.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.

### Prerequisites

In order to run the Bot, you need to set the following Environment Variables and a MongoDB instance:

```
NFLBOT_DISCORDTOKEN: API Token for the Discord Bot Application
NFLBOT_BUNGIETOKEN: API Token for the Bungie Application
NFLBOT_CLANID: ID of your Destiny Clan
NFLBOT_MONGOSTRING: Connection String to connect to your MongoDB instance
```

You'll also need Visual Studio 2019 with .NET Core 2.2 installed in order to run the Bot from VS and develop for it.

## Deployment

Use the default "Publish" functionality from Visual Studio to create release binaries.

## Commands

The following Commands are available:

* !nfl help - Shows a list of available commands
* !nfl top {0} - Shows the overall top scores across all Ordeal Activities, {0} being the amount of results you want.
* !nfl score {0} {ADEPT/HERO/MASTER/LEGEND} - Shows the Top10 of a given Nightfall. {0} being the Nightfall Name.
* !nfl init - Requires Bot Owner. Resets the Score database.
* !nfl load - Requires Bot Owner. Forces loading of new scores.
* !nfl player {0} {1} {2} {ADEPT/HERO/MASTER/LEGEND} - Gets a players scores. {0} is the player name. {1} the amount of results. {2} the activity name. (Everything but the name is optional)

## Built With

* [DiscordBotBase](https://github.com/foxbot/DiscordBotBase) - The base template for the Destiny Bot
* [BungieNetApi](https://github.com/madreflection/MadReflection.BungieNetApi) - .NET Wrapper for Bungie's API

## Contributing

Please read [CONTRIBUTING.md](https://github.com/KiaArmani/NFLBot/CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/your/project/tags). 

## Authors

* **Christopher F.** - *DiscordBotBase* - [foxbot](https://github.com/foxbot/)
* **Benn Benson** - *MadReflection.BungieNetApi* - [madreflection](https://github.com/madreflection/)
* **Kia Armani** - *NFLBot* - [KiaArmani](https://github.com/KiaArmani/)

See also the list of [contributors](https://github.com/KiaArmani/NFLBot/contributors) who participated in this project.

## License

This project is licensed under the ISC & BSD 3-Clause License - see the [LICENSE.md](LICENSE.md) file for details
