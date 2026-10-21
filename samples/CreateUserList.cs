// Copyright 2026 Google LLC
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

using System.Reflection;
using CommandLine;
using Google.Ads.DataManager.V1;
using Google.Api.Gax.Grpc;
using Google.Protobuf.Reflection;
using Google.Protobuf.WellKnownTypes;
using static Google.Ads.DataManager.V1.IngestedUserListInfo.Types;
using static Google.Ads.DataManager.V1.MobileIdInfo.Types;
using static Google.Ads.DataManager.V1.ProductAccount.Types;

namespace Google.Ads.DataManager.Samples
{
    // <summary>
    // Sample of creating a <see cref="UserList" /> in an operating account.
    // </summary>
    public class CreateUserList
    {
        [Verb("create-user-list", HelpText = "Creates a UserList in an operating account.")]
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
                "uploadKeyType",
                Required = false,
                Default = UploadKeyType.ContactId,
                HelpText = "Upload key type of the user list (ContactId, UserId, or MobileId). Defaults to ContactId."
            )]
            public UploadKeyType UploadKeyType { get; set; }

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

            [Option(
                "keySpace",
                Required = false,
                HelpText = "Key space of mobile IDs (Android or Ios). Required when --uploadKeyType is MobileId."
            )]
            public KeySpace? KeySpace { get; set; }

            [Option(
                "appId",
                Required = false,
                HelpText = "String that uniquely identifies a mobile application from which the data was collected (for example, 'com.labpixies.flood' for Android or '476943146' for iOS). Required when --uploadKeyType is MobileId."
            )]
            public string? AppId { get; set; }

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
                options.ValidateOnly,
                options.UploadKeyType,
                options.LoginAccountType,
                options.LoginAccountId,
                options.LinkedAccountType,
                options.LinkedAccountId,
                options.KeySpace,
                options.AppId
            );
        }

        // [START create-user-list]
        // Runs the example.
        private void RunExample(
            AccountType operatingAccountType,
            string operatingAccountId,
            bool validateOnly,
            UploadKeyType uploadKeyType = UploadKeyType.ContactId,
            AccountType? loginAccountType = null,
            string? loginAccountId = null,
            AccountType? linkedAccountType = null,
            string? linkedAccountId = null,
            KeySpace? keySpace = null,
            string? appId = null
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
            if (uploadKeyType == UploadKeyType.MobileId)
            {
                if (!keySpace.HasValue || string.IsNullOrEmpty(appId))
                {
                    throw new ArgumentException(
                        "Must specify both keySpace and appId when uploadKeyType is MobileId"
                    );
                }
            }
            else if (keySpace.HasValue || !string.IsNullOrEmpty(appId))
            {
                throw new ArgumentException(
                    "keySpace and appId are only supported when uploadKeyType is MobileId"
                );
            }

            // Optional: Sets headers to specify the login-account and linked-account if your
            // credentials aren't for a user in the operating account (for example, when
            // authenticating through a Google Ads manager account or Data Partner account).
            CallSettings? callSettings = null;
            if (loginAccountType.HasValue && loginAccountId != null)
            {
                callSettings = callSettings.WithHeader(
                    "login-account",
                    // Uses the AccountName utility to construct the login-account resource name.
                    AccountName.Format(GetEnumOriginalName(loginAccountType.Value), loginAccountId)
                );
            }
            if (linkedAccountType.HasValue && linkedAccountId != null)
            {
                callSettings = callSettings.WithHeader(
                    "linked-account",
                    // Uses the AccountName utility to construct the linked-account resource name.
                    AccountName.Format(
                        GetEnumOriginalName(linkedAccountType.Value),
                        linkedAccountId
                    )
                );
            }

            // Configures the Customer Match user list.
            // For more details on constructing a UserList, see:
            // https://developers.google.com/data-manager/api/devguides/audiences/google-ads/customer-match/create-audience
            var ingestedUserListInfo = new IngestedUserListInfo
            {
                // Sets to a list containing a single UploadKeyType.
                UploadKeyTypes = { uploadKeyType },
            };

            switch (uploadKeyType)
            {
                case UploadKeyType.ContactId:
                    // Use CONTACT_ID for hashed email addresses, phone numbers, or physical
                    // addresses.
                    ingestedUserListInfo.ContactIdInfo = new ContactIdInfo
                    {
                        DataSourceType = DataSourceType.FirstParty,
                    };
                    break;
                case UploadKeyType.UserId:
                    // Use USER_ID for first-party user IDs assigned to logged-in users.
                    // Note: Display & Video 360 does not support USER_ID lists.
                    ingestedUserListInfo.UserIdInfo = new UserIdInfo
                    {
                        DataSourceType = DataSourceType.FirstParty,
                    };
                    break;
                case UploadKeyType.MobileId:
                    // Use MOBILE_ID for mobile advertising IDs.
                    ingestedUserListInfo.MobileIdInfo = new MobileIdInfo
                    {
                        DataSourceType = DataSourceType.FirstParty,
                        KeySpace = keySpace!.Value,
                        AppId = appId,
                    };
                    break;
                default:
                    throw new ArgumentException($"Unsupported upload key type: {uploadKeyType}");
            }

            string uploadKeyTypeName = GetEnumOriginalName(uploadKeyType);
            var userList = new UserList
            {
                // Required. The display name must be unique among all lists owned by the account.
                DisplayName =
                    $"Customer Match ({uploadKeyTypeName}) - {DateTime.Now:yyyy-MM-dd HH:mm:ss}",
                Description = $"Customer Match audience for {uploadKeyTypeName}",
                // Optional. The duration an audience member remains in the list after being added.
                // Must correspond to whole days (defaults to maximum if not set).
                MembershipDuration = Duration.FromTimeSpan(TimeSpan.FromDays(30)),
                // Required for a Customer Match audience.
                IngestedUserListInfo = ingestedUserListInfo,
            };

            // Creates a client for UserListService.
            UserListServiceClient userListServiceClient = UserListServiceClient.Create();

            // Builds the request targeting the parent operating account.
            var request = new CreateUserListRequest
            {
                Parent = AccountName.Format(
                    GetEnumOriginalName(operatingAccountType),
                    operatingAccountId
                ),
                UserList = userList,
                // Sets validate_only. If true, then the Data Manager API only validates the
                // request but doesn't apply changes.
                ValidateOnly = validateOnly,
            };

            // Sends the request with any configured header settings.
            UserList response = userListServiceClient.CreateUserList(request, callSettings);

            // Logs the created user list. When validate_only is false, response.Id contains the
            // created user list ID, which you pass as the ProductDestinationId in subsequent
            // ingest or remove requests.
            Console.WriteLine($"Response:\n{response}");
        }

        // Returns the original proto enum name (for example, "GOOGLE_ADS" for
        // AccountType.GoogleAds).
        private static string GetEnumOriginalName<T>(T enumValue)
            where T : struct, System.Enum =>
            typeof(T)
                .GetField(enumValue.ToString())
                ?.GetCustomAttribute<OriginalNameAttribute>()
                ?.Name
            ?? enumValue.ToString();
        // [END create-user-list]
    }
}
