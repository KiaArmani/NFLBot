# NFLBot

A Discord Bot that collects Nightfall Activities of your Destiny Clan and providing commands for scoreboards.

## Getting Started

These instructions will get you a copy of the project up and running on your local machine for development and testing purposes. See deployment for notes on how to deploy the project on a live system.

### Prerequisites

What things you need to install the software and how to install them
In order to run the Bot, you need to set the following Environment Variables:

```
NFLBOT_DISCORDTOKEN: API Token for the Discord Bot Application
NFLBOT_BUNGIETOKEN: API Token for the Bungie Application
NFLBOT_CLANID: ID of your Destiny Clan
(Optional) NFLBOT_ATLASTOKEN: If using the Cloud Atlas Connection, the Token for your User.
```

You'll also need Visual Studio 2019 with .NET Core 3.0 installed in order to run the Bot from VS and develop for it.

## Deployment

Add additional notes about how to deploy this on a live system

## Built With

* [DiscordBotBase](https://github.com/foxbot/DiscordBotBase) - The base template for the Destiny Bot
* [BungieNetApi](https://github.com/madreflection/MadReflection.BungieNetApi) - .NET Wrapper for Bungie's API

## Contributing

Please read [CONTRIBUTING.md](https://github.com/KiaArmani/NFLBot/CONTRIBUTING.md) for details on our code of conduct, and the process for submitting pull requests to us.

## Versioning

We use [SemVer](http://semver.org/) for versioning. For the versions available, see the [tags on this repository](https://github.com/your/project/tags). 

## Authors

* **Christopher F. <foxbot@protonmail.com>** - *DiscordBotBase* - [foxbot](https://github.com/foxbot/)
* **Benn Benson** - *MadReflection.BungieNetApi* - [madreflection](https://github.com/madreflection/)
* **Kia Armani** - *NFLBot* - [KiaArmani](https://github.com/KiaArmani/)

See also the list of [contributors](https://github.com/KiaArmani/NFLBot/contributors) who participated in this project.

## License

This project is licensed under the ISC & BSD 3-Clause License - see the [LICENSE.md](LICENSE.md) file for details
