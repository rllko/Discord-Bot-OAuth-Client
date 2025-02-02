  # Discord Bot OAuth Client

This project is a template for creating a Discord bot with a custom OAuth implementation using the Discord.net Framework.

## Features

- Custom OAuth implementation
- Easy-to-use template for creating Discord bots
- Built with the Discord.net Framework

## Prerequisites

- Docker installed on your machine
- Discord Developer Portal account to create a bot and get the necessary credentials

## Installation

1. Clone the repository:
    ```bash
    git clone https://github.com/rllko/Discord-Bot-OAuth-Client.git
    cd Discord-Bot-OAuth-Client
    ```

3. Update `docker-compose.yml` file with your bot and server credentials:
    ```yml
     environment:
      - discord_token=TOKEN HERE
      - guildId=SERVER ID
      - clientId
      - clientSecret
      - response_type
      - baseUrl
      - scope
      - code_challenge_method
    ```

## Usage

1. Run the project:
    ```docker
    docker compose up
    ```

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Acknowledgements

- [Discord.net](https://github.com/discord-net/Discord.Net) for the framework
