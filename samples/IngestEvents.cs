// Copyright 2025 Google LLC
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Text.Json;
using CommandLine;
using Google.Ads.DataManager.Util;
using Google.Ads.DataManager.V1;
using Google.Protobuf.WellKnownTypes;
using static Google.Ads.DataManager.V1.ProductAccount.Types;

namespace Google.Ads.DataManager.Samples
{
    // <summary>
    // Sends an <see cref="IngestEventsRequest" /> without using encryption.
    //
    // Event data is read from a data file. See the <c>events_1.json</c> file in the
    // <c>sampledata</c> directory for an example.
    // </summary>
    public class IngestEvents
    {
        private static readonly int MaxEventsPerRequest = 2_000;

        [Verb("ingest-events", HelpText = "Sends an IngestEventsRequest without using encryption.")]
        public class Options
        {
            [Option(
                "operatingAccountType",
                Required = true,
                HelpText = "Account type of the operating account"
            )]
            public AccountType OperatingAccountType { get; set; }

            [Option(
                "operatingAccountId",
                Required = true,
                HelpText = "ID of the operating account"
            )]
            public string OperatingAccountId { get; set; } = null!;

            [Option(
                "loginAccountType",
                Required = false,
                HelpText = "Account type of the login account"
            )]
            public AccountType? LoginAccountType { get; set; }

            [Option("loginAccountId", Required = false, HelpText = "ID of the login account")]
            public string? LoginAccountId { get; set; }

            [Option(
                "linkedAccountProduct",
                Required = false,
                HelpText = "Account type of the linked account"
            )]
            public AccountType? LinkedAccountType { get; set; }

            [Option("linkedAccountId", Required = false, HelpText = "ID of the linked account")]
            public string? LinkedAccountId { get; set; }

            [Option(
                "conversionActionId",
                Required = true,
                HelpText = "ID of the conversion action"
            )]
            public string ConversionActionId { get; set; } = null!;

            [Option(
                "jsonFile",
                Required = true,
                HelpText = "JSON file containing user data to ingest"
            )]
            public string JsonFile { get; set; } = null!;

            [Option(
                "validateOnly",
                Default = true,
                HelpText = "Whether to enable validateOnly on the request"
            )]
            public bool ValidateOnly { get; set; }
        }

        public void Run(Options options)
        {
            RunExample(
                options.OperatingAccountType,
                options.OperatingAccountId,
                options.LoginAccountType,
                options.LoginAccountId,
                options.LinkedAccountType,
                options.LinkedAccountId,
                options.ConversionActionId,
                options.JsonFile,
                options.ValidateOnly
            );
        }

        private void RunExample(
            AccountType operatingAccountType,
            string operatingAccountId,
            AccountType? loginAccountType,
            string? loginAccountId,
            AccountType? linkedAccountType,
            string? linkedAccountId,
            string conversionActionId,
            string jsonFile,
            bool validateOnly
        )
        {
            if (loginAccountId == null ^ loginAccountType == null)
            {
                throw new ArgumentException(
                    "Must specify either both or neither of login account ID and login account "
                        + "type"
                );
            }
            if (linkedAccountId == null ^ linkedAccountType == null)
            {
                throw new ArgumentException(
                    "Must specify either both or neither of linked account ID and linked account "
                        + "type"
                );
            }

            // Reads member data from the data file.
            List<EventRecord> eventRecords = ReadEventData(jsonFile);
            // Gets an instance of the UserDataFormatter for normalizing and formatting the data.
            UserDataFormatter userDataFormatter = new UserDataFormatter();

            // Builds the events collection for the request.
            var events = new List<Event>();
            foreach (var eventRecord in eventRecords)
            {
                var eventBuilder = new Event();

                try
                {
                    eventBuilder.EventTimestamp = Timestamp.FromDateTime(
                        DateTime.Parse(eventRecord.Timestamp ?? "").ToUniversalTime()
                    );
                }
                catch (FormatException)
                {
                    Console.WriteLine(
                        $"Skipping event with invalid timestamp: {eventRecord.Timestamp}"
                    );
                    continue;
                }

                if (string.IsNullOrEmpty(eventRecord.TransactionId))
                {
                    Console.WriteLine("Skipping event with no transaction ID");
                    continue;
                }
                eventBuilder.TransactionId = eventRecord.TransactionId;

                if (!string.IsNullOrEmpty(eventRecord.EventSource))
                {
                    if (
                        System.Enum.TryParse(
                            eventRecord.EventSource,
                            true,
                            out EventSource eventSource
                        )
                    )
                    {
                        eventBuilder.EventSource = eventSource;
                    }
                    else
                    {
                        Console.WriteLine(
                            $"Skipping event with invalid event source: {eventRecord.EventSource}"
                        );
                        continue;
                    }
                }

                if (!string.IsNullOrEmpty(eventRecord.Gclid))
                {
                    eventBuilder.AdIdentifiers = new AdIdentifiers { Gclid = eventRecord.Gclid };
                }

                if (!string.IsNullOrEmpty(eventRecord.Currency))
                {
                    eventBuilder.Currency = eventRecord.Currency;
                }

                if (eventRecord.Value.HasValue)
                {
                    eventBuilder.ConversionValue = eventRecord.Value.Value;
                }

                var userDataBuilder = new UserData();

                // Adds a UserIdentifier for each valid email address for the eventRecord.
                if (eventRecord.Emails != null)
                {
                    foreach (var email in eventRecord.Emails)
                    {
                        try
                        {
                            string preparedEmail = userDataFormatter.ProcessEmailAddress(
                                email,
                                UserDataFormatter.Encoding.Hex
                            );
                            // Adds an email address identifier with the encoded email hash.
                            userDataBuilder.UserIdentifiers.Add(
                                new UserIdentifier { EmailAddress = preparedEmail }
                            );
                        }
                        catch (ArgumentException)
                        {
                            // Skips invalid input.
                            continue;
                        }
                    }
                }

                // Adds a UserIdentifier for each valid phone number for the eventRecord.
                if (eventRecord.PhoneNumbers != null)
                {
                    foreach (var phoneNumber in eventRecord.PhoneNumbers)
                    {
                        try
                        {
                            string preparedPhoneNumber = userDataFormatter.ProcessPhoneNumber(
                                phoneNumber,
                                UserDataFormatter.Encoding.Hex
                            );
                            // Adds a phone number identifier with the encoded phone hash.
                            userDataBuilder.UserIdentifiers.Add(
                                new UserIdentifier { PhoneNumber = preparedPhoneNumber }
                            );
                        }
                        catch (ArgumentException)
                        {
                            // Skips invalid input.
                            continue;
                        }
                    }
                }

                if (userDataBuilder.UserIdentifiers.Any())
                {
                    eventBuilder.UserData = userDataBuilder;
                }
                events.Add(eventBuilder);
            }

            // Builds the Destination for the request.
            var destinationBuilder = new Destination
            {
                OperatingAccount = new ProductAccount
                {
                    AccountType = operatingAccountType,
                    AccountId = operatingAccountId,
                },
                ProductDestinationId = conversionActionId,
            };

            if (loginAccountType.HasValue && loginAccountId != null)
            {
                destinationBuilder.LoginAccount = new ProductAccount
                {
                    AccountType = loginAccountType.Value,
                    AccountId = loginAccountId,
                };
            }

            if (linkedAccountType.HasValue && linkedAccountId != null)
            {
                destinationBuilder.LinkedAccount = new ProductAccount
                {
                    AccountType = linkedAccountType.Value,
                    AccountId = linkedAccountId,
                };
            }

            IngestionServiceClient ingestionServiceClient = IngestionServiceClient.Create();

            int requestCount = 0;

            // Batches requests to send up to the maximum number of events per request.
            for (var i = 0; i < events.Count; i += MaxEventsPerRequest)
            {
                IEnumerable<Event> batch = events.Skip(i).Take(MaxEventsPerRequest);
                requestCount++;
                var request = new IngestEventsRequest
                {
                    Destinations = { destinationBuilder },
                    // Adds events from the current batch.
                    Events = { batch },
                    Consent = new Consent
                    {
                        AdPersonalization = ConsentStatus.ConsentGranted,
                        AdUserData = ConsentStatus.ConsentGranted,
                    },
                    // Sets validate_only. If true, then the Data Manager API only validates the
                    // request but doesn't apply changes.
                    ValidateOnly = validateOnly,
                    Encoding = V1.Encoding.Hex,
                };

                // Sends the data to the Data Manager API.
                IngestEventsResponse response = ingestionServiceClient.IngestEvents(request);
                Console.WriteLine($"Response for request #{requestCount}:\n{response}");
            }
            Console.WriteLine($"# of requests sent: {requestCount}");
        }

        private class EventRecord
        {
            public List<string>? Emails { get; set; }
            public List<string>? PhoneNumbers { get; set; }
            public string? Timestamp { get; set; }
            public string? TransactionId { get; set; }
            public string? EventSource { get; set; }
            public double? Value { get; set; }
            public string? Currency { get; set; }
            public string? Gclid { get; set; }
        }

        private List<EventRecord> ReadEventData(string jsonFile)
        {
            string jsonString = File.ReadAllText(jsonFile);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<List<EventRecord>>(jsonString, options)
                ?? new List<EventRecord>();
        }
    }
}
