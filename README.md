# Data Manager API utility library and samples for .NET

[![NuGet version](https://img.shields.io/nuget/v/Google.Ads.DataManager.Util.svg)](https://www.nuget.org/packages/Google.Ads.DataManager.Util)

Utility library and code samples for working with the
[Data Manager API](https://developers.google.com/data-manager/api) and .NET.

## Requirements

- .NET Standard 2.0+ (for the `Google.Ads.DataManager.Util` library)
- .NET 8.0+ (for running the samples and tests)

## Setup instructions

The `Google.Ads.DataManager.Util` utility library is published to
[NuGet](https://www.nuget.org/packages/Google.Ads.DataManager.Util).

Install the library using the .NET CLI:

```shell
dotnet add package Google.Ads.DataManager.Util
```

Or using the Package Manager Console in Visual Studio:

```powershell
Install-Package Google.Ads.DataManager.Util
```

For complete instructions on setting up API access and installing the client and
utility libraries, see the
[Set up API access](https://developers.google.com/data-manager/api/devguides/quickstart/set-up-access)
and
[Install a client library](https://developers.google.com/data-manager/api/devguides/quickstart/install-library#.net)
guides.

## Repository structure

- [`Google.Ads.DataManager.Util`](Google.Ads.DataManager.Util): Source code and
  tests for the `Google.Ads.DataManager.Util` NuGet package. Use the utilities
  in the library to help with common tasks like formatting, hashing, encrypting,
  and encoding data for Data Manager API requests.

- [`samples`](samples): Code samples demonstrating how to construct and send
  requests to the Data Manager API using the
  [`Google.Ads.DataManager.V1`](https://www.nuget.org/packages/Google.Ads.DataManager.V1)
  client library and the `Google.Ads.DataManager.Util` utility library.

## Run samples

To run a sample, invoke the script using the command line. You can pass
arguments to the script in one of two ways:

### 1. Explicitly, on the command line

The first argument must be the name of the sample. The name is the simple class name, converted to
lowercase and with a hyphen (`-`) between each capitalized word. For example,
the name of the sample for the `IngestEvents` class is `ingest-events`.

```shell
dotnet run --project samples/DataManager.Samples.csproj \
  ingest-events \
  --operatingAccountType <operating_account_type> \
  --operatingAccountId <operating_account_id> \
  --conversionActionId <conversion_action_id> \
  --jsonFile '</path/to/your/file>'
```

Quote any argument that contains a space.

### 2. Using an arguments file

You can also save arguments in a file. Don't quote argument values in your
arguments file, even if the value contains a space.

```
ingest-events
--operatingAccountType
<operating_account_type>
--operatingAccountId
<operating_account_id>
--conversionActionId
<conversion_action_id>
--jsonFile
</path/to/your/file>
```

Then, run the sample by passing the file path prefixed with the `@` character.

```shell
dotnet run --project samples/DataManager.Samples.csproj @</path/to/your/file>
```

## Issue tracker

- https://github.com/googleads/data-manager-dotnet/issues

## Contributing

Contributions welcome! See the [Contributing Guide](CONTRIBUTING.md).

## Authors

- [Josh Radcliff](https://github.com/jradcliff)
- [Lindsey Volta](https://github.com/lindsey-volta)
