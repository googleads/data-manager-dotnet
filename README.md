# Data Manager API utilities and samples for .NET

Utility library and code samples for working with the
[Data Manager API](https://developers.google.com/data-manager/api) and .NET.

## Setup instructions

https://developers.google.com/data-manager/api/get-started/set-up-access#dotnet

## Repository structure

- [`Google.Ads.DataManager.Util`](Google.Ads.DataManager.Util): Source code and
  tests for the utility library.

  Follow the setup instructions to declare a dependency on the current version
  of `Google.Ads.DataManager.Util` in your project. Use the utilities
  in the library to help with common tasks like formatting, hashing, encrypting,
  and encoding data for Data Manager API requests.

- [`samples`](samples): Code samples for working with the Data Manager API and
  the utility library.

  The `DataManager.Samples` project demonstrates how to set up a project that
  depends on the Data Manager API client library and the `Google.Ads.DataManager.Util` library.
  Check out the [samples](samples) directory for code samples that construct and send requests to
  the Data Manager API.

## Run samples

To run a sample, invoke the script using the command line. You can pass
arguments to the script in one of two ways:

### 1.  Explicitly, on the command line

The first argument must be the name of the sample. The name is the simple class name, converted to
lowercase and with a hyphen (`-`) between each capitalized word. For example,
the name of the sample for the `IngestEvents` class is `ingest-events`.

```shell
dotnet run --project samples/DataManager.Samples/csroj \
  ingest-events \
  --operatingAccountType <operating_account_type> \
  --operatingAccountId <operating_account_id> \
  --conversionActionId <conversion_action_id> \
  --jsonFile '</path/to/your/file>'
```

Quote any argument that contains a space.

### 2.  Using an arguments file

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
