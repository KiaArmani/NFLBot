# XurSuite

[![FOSSA Status](https://app.fossa.com/api/projects/git%2Bgithub.com%2Fd2tools%2FNFLBot.svg?type=shield)](https://app.fossa.com/projects/git%2Bgithub.com%2FKiaArmani%2FXurSuite?ref=badge_shield)

A collection of Microservices harvesting player activity data from a Clan in Destiny 2 and processing them to allow for custom challenges to be provided to clan members.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.

### Prerequisites

In order to run any given service, you need to set the following Environment Variables and a MongoDB instance:

```
XUR_{SERVICENAME}_DISCORDTOKEN: API Token for the Discord Bot Application
XUR_{SERVICENAME}_BUNGIETOKEN: API Token for the Bungie Application
XUR_{SERVICENAME}_CLANID: ID of your Destiny Clan
XUR_{SERVICENAME}_MONGOSTRING: Connection String to connect to your MongoDB instance
```

You'll also need Visual Studio 2019 with .NET Core 3.0 installed in order to run the Bot from VS and develop for it.

## Deployment

Use the default "Publish" functionality from Visual Studio to create release binaries and / or publish a docker image.

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
