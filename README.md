# Landerist

Open project for the decentralization of real estate listings using the following methodology:

1. Locate websites of real estate agencies by country that may contain property listings.
2. Download the data from the real estate listings and standardize it using artificial intelligence.
3. Distribute the dataset for free in [ORELS](https://github.com/techjb/Open-Real-Estate-Listings-Schema) format.
4. Keep the dataset updated daily.

Visit [landerist.com](https://landerist.com)

## Installation

Download the project and open it in Visual Studio. Copy `appsettings.example.json` to
`appsettings.Local.json` and fill in the settings needed by your environment. The local
file is ignored by Git and must not be committed.

Settings can also be supplied with environment variables prefixed with `LANDERIST__`.
For example, `LANDERIST__DATABASE_NAME` overrides `DATABASE_NAME` from the JSON files.
Environment variables take precedence over local JSON settings.

Open `Database.sql`, update its local paths, and run it in SQL Server to initialize the database.

## Contributing

Pull requests are welcome. For major changes, please open an issue first
to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License

[MIT](https://choosealicense.com/licenses/mit/)