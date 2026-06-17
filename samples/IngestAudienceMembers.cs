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
using static Google.Ads.DataManager.V1.ProductAccount.Types;

namespace Google.Ads.DataManager.Samples
{
    // <summary>
    // Sends a <see cref="IngestAudienceMembersRequest" /> without using encryption.
    //
    // User data is read from a data file. See the <c>audience_members_1.json</c> file in the
    // <c>sampledata</c> directory for an example.
    // </summary>
    public class IngestAudienceMembers
    {
        private static readonly int MaxMembersPerRequest = 10_000;

        [Verb(
            "ingest-audience-members",
            HelpText = "Sends an IngestAudienceMembersRequest without using encryption."
        )]
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
                "linkedAccountType",
                Required = false,
                HelpText = "Account type of the linked account"
            )]
            public AccountType? LinkedAccountType { get; set; }

            [Option("linkedAccountId", Required = false, HelpText = "ID of the linked account")]
            public string? LinkedAccountId { get; set; }

            [Option("audienceId", Required = true, HelpText = "ID of the audience")]
            public string AudienceId { get; set; } = null!;

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
                options.AudienceId,
                options.JsonFile,
                options.ValidateOnly
            );
        }

        // Runs the example.
        private void RunExample(
            AccountType operatingAccountType,
            string operatingAccountId,
            AccountType? loginAccountType,
            string? loginAccountId,
            AccountType? linkedAccountType,
            string? linkedAccountId,
            string audienceId,
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

            // Reads the audience members from the JSON file.
            List<Member> memberList = ReadMemberData(jsonFile);

            // Creates a factory that will be used to generate the appropriate data manager.
            var userDataFormatter = new UserDataFormatter();

            var audienceMembers = new List<AudienceMember>();

            // Processes each batch of audience members.
            foreach (var member in memberList)
            {
                var userDataBuilder = new UserData();

                // Adds a UserIdentifier for each valid email address for the member.
                foreach (var email in member.Emails)
                {
                    try
                    {
                        string processedEmail = userDataFormatter.ProcessEmailAddress(
                            email,
                            UserDataFormatter.Encoding.Hex
                        );
                        // Sets the email address identifier to the encoded hash.
                        userDataBuilder.UserIdentifiers.Add(
                            new UserIdentifier { EmailAddress = processedEmail }
                        );
                    }
                    catch (ArgumentException)
                    {
                        // Skips invalid input.
                        continue;
                    }
                }

                // Adds a UserIdentifier for each valid phone number for the member.
                foreach (var phoneNumber in member.PhoneNumbers)
                {
                    try
                    {
                        string processedPhoneNumber = userDataFormatter.ProcessPhoneNumber(
                            phoneNumber,
                            UserDataFormatter.Encoding.Hex
                        );
                        // Sets the phone number identifier to the encoded hash.
                        userDataBuilder.UserIdentifiers.Add(
                            new UserIdentifier { PhoneNumber = processedPhoneNumber }
                        );
                    }
                    catch (ArgumentException)
                    {
                        // Skips invalid input.
                        continue;
                    }
                }

                if (userDataBuilder.UserIdentifiers.Any())
                {
                    audienceMembers.Add(new AudienceMember { UserData = userDataBuilder });
                }
            }

            // Builds the Destination for the request.
            var destinationBuilder = new Destination
            {
                // The destination account for the data.
                OperatingAccount = new ProductAccount
                {
                    AccountType = operatingAccountType,
                    AccountId = operatingAccountId,
                },
                // The ID of the user list that is being updated.
                ProductDestinationId = audienceId,
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

            // Batches requests to send up to the maximum number of audience members per request.
            for (var i = 0; i < audienceMembers.Count; i += MaxMembersPerRequest)
            {
                IEnumerable<AudienceMember> membersBatch = audienceMembers
                    .Skip(i)
                    .Take(MaxMembersPerRequest);
                requestCount++;
                // Builds the request.
                var request = new IngestAudienceMembersRequest
                {
                    Destinations = { destinationBuilder },
                    // Adds members from the current batch.
                    AudienceMembers = { membersBatch },
                    Consent = new Consent
                    {
                        AdPersonalization = ConsentStatus.ConsentGranted,
                        AdUserData = ConsentStatus.ConsentGranted,
                    },
                    // Sets validate_only. If true, then the Data Manager API only validates the
                    // request but doesn't apply changes.
                    ValidateOnly = validateOnly,
                    Encoding = V1.Encoding.Hex,
                    TermsOfService = new TermsOfService
                    {
                        CustomerMatchTermsOfServiceStatus = TermsOfServiceStatus.Accepted,
                    },
                };

                // Logs the request.
                Console.WriteLine($"Request #{requestCount}:\n{request}");

                // Sends the data to the Data Manager API.
                IngestAudienceMembersResponse response =
                    ingestionServiceClient.IngestAudienceMembers(request);
                Console.WriteLine($"Response for request #{requestCount}:\n{response}");
            }
            Console.WriteLine($"# of requests sent: {requestCount}");
        }

        private class Member
        {
            public List<string> Emails { get; set; } = new List<string>();
            public List<string> PhoneNumbers { get; set; } = new List<string>();
        }

        private List<Member> ReadMemberData(string jsonFile)
        {
            string jsonString = File.ReadAllText(jsonFile);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<List<Member>>(jsonString, options)
                ?? new List<Member>();
        }
    }
}
