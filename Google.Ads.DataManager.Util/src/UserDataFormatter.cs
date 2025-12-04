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

using System.Globalization;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace Google.Ads.DataManager.Util
{
    /// <summary>
    /// Utility for normalizing and formatting user data.
    /// <p>Methods fall into two categories:</p>
    /// <ol>
    ///   <li><em>Convenience methods</em> named <code>Process...(...)</code> that handle
    ///   <em>all</em> formatting, hashing and encoding of a specific type of data in one method
    ///   call.</li>
    ///   <li><em>Fine-grained methods</em> such as <code>Format...(...)</code>, encoding, and
    ///   hashing methods that perform a specific data processing step.</li>
    /// </ol>
    /// <p>Using the convenience methods is easier, less error-prone, and more concise. For example,
    /// compare the two approaches to format, hash, and encode an email address:</p>
    /// <pre>
    ///   // Uses a convenience method.
    ///   string result1 = formatter.ProcessEmailAddress(emailAddress, Encoding.Hex);
    ///
    ///   // Uses a chain of fine-grained method calls.
    ///   string result =
    ///       formatter.HexEncode(formatter.HashString(formatter.FormatEmailAddress(emailAddress)))
    /// </pre>
    /// <p>Methods throw <see cref="ArgumentException"/> when passed invalid input. Since arguments
    /// to these methods contain user data, exception messages <em>don't</em> include the argument
    /// values.</p>
    /// <p>Instances of this class are <em>not</em> thread-safe.</p>
    /// </summary>
    public class UserDataFormatter
    {
        private readonly SHA256 _sha256;

        private static readonly Regex WhitespacePattern = new Regex(@"\s");
        private static readonly Regex PeriodPattern = new Regex(@"\.");
        private static readonly Regex NonDigitPattern = new Regex(@"\D");
        private static readonly Regex GivenNamePrefixPattern = new Regex(
            @"(?:mr|mrs|ms|dr)\.(?:\s|$)"
        );
        private static readonly Regex FamilyNameSuffixPattern = new Regex(
            @"(?:,\s*|\s+)(?:jr\.|sr\.|2nd|3rd|ii|iii|iv|v|vi|cpa|dc|dds|vm|jd|md|phd)\s?$"
        );
        private static readonly Regex AllUppercaseCharsPattern = new Regex(@"^[A-Z]+$");

        public enum Encoding
        {
            Hex,
            Base64,
        }

        public UserDataFormatter()
        {
            _sha256 = SHA256.Create();
        }

        /// <summary>
        /// Returns the provided email address, normalized and formatted.
        /// </summary>
        /// <param name="emailAddress">The email address to format.</param>
        /// <exception cref="ArgumentException">If emailAddress is invalid.</exception>
        public string FormatEmailAddress(string emailAddress)
        {
            if (emailAddress == null)
            {
                throw new ArgumentNullException(nameof(emailAddress), "Null email address");
            }
            emailAddress = emailAddress.Trim();
            if (string.IsNullOrEmpty(emailAddress))
            {
                throw new ArgumentException("Empty or blank email address", nameof(emailAddress));
            }
            if (WhitespacePattern.IsMatch(emailAddress))
            {
                throw new ArgumentException(
                    "Email contains intermediate whitespace",
                    nameof(emailAddress)
                );
            }

            string[] emailParts = emailAddress.ToLower(CultureInfo.InvariantCulture).Split('@');
            if (emailParts.Length != 2)
            {
                throw new ArgumentException(
                    "Email is not of the form user@domain",
                    nameof(emailAddress)
                );
            }

            string username = emailParts[0];
            string domain = emailParts[1];

            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException(
                    "Email address without the domain is empty",
                    nameof(emailAddress)
                );
            }
            if (string.IsNullOrEmpty(domain))
            {
                throw new ArgumentException(
                    "Domain of email address is empty",
                    nameof(emailAddress)
                );
            }

            if (domain == "gmail.com" || domain == "googlemail.com")
            {
                username = PeriodPattern.Replace(username, "");
            }

            if (string.IsNullOrEmpty(username))
            {
                throw new ArgumentException(
                    "Email address without the domain name is empty " + "after normalization",
                    nameof(emailAddress)
                );
            }

            return $"{username}@{domain}";
        }

        /// <summary>
        /// Returns the provided phone number, normalized and formatted.
        /// </summary>
        /// <param name="phoneNumber">The phone number to format.</param>
        /// <exception cref="ArgumentException">If phoneNumber is invalid.</exception>
        public string FormatPhoneNumber(string phoneNumber)
        {
            if (phoneNumber == null)
            {
                throw new ArgumentNullException(nameof(phoneNumber), "Null phone number");
            }
            phoneNumber = phoneNumber.Trim();
            if (string.IsNullOrEmpty(phoneNumber))
            {
                throw new ArgumentException("Empty or blank phone number", nameof(phoneNumber));
            }

            string digitsOnly = NonDigitPattern.Replace(phoneNumber, "");

            if (string.IsNullOrEmpty(digitsOnly))
            {
                throw new ArgumentException("Phone number contains no digits", nameof(phoneNumber));
            }

            return $"+{digitsOnly}";
        }

        /// <summary>
        /// Returns the provided given name, normalized and formatted.
        /// </summary>
        /// <param name="givenName">The given name to format.</param>
        /// <exception cref="ArgumentException">If givenName is invalid.</exception>
        public string FormatGivenName(string givenName)
        {
            if (givenName == null)
            {
                throw new ArgumentNullException(nameof(givenName), "Null given name");
            }
            givenName = givenName.Trim();
            if (string.IsNullOrEmpty(givenName))
            {
                throw new ArgumentException("Empty or blank given name", nameof(givenName));
            }
            givenName = givenName.ToLower(CultureInfo.InvariantCulture);
            string withoutPrefix = GivenNamePrefixPattern.Replace(givenName, "").Trim();
            if (string.IsNullOrEmpty(withoutPrefix))
            {
                throw new ArgumentException(
                    "Given name consists solely of a prefix",
                    nameof(givenName)
                );
            }
            return withoutPrefix;
        }

        /// <summary>
        /// Returns the provided family name, normalized and formatted.
        /// </summary>
        /// <param name="familyName">The family name to format.</param>
        /// <exception cref="ArgumentException">If familyName is invalid.</exception>
        public string FormatFamilyName(string familyName)
        {
            if (familyName == null)
            {
                throw new ArgumentNullException(nameof(familyName), "Null family name");
            }
            familyName = familyName.Trim();
            if (string.IsNullOrEmpty(familyName))
            {
                throw new ArgumentException("Empty or blank family name", nameof(familyName));
            }
            familyName = familyName.ToLower(CultureInfo.InvariantCulture);

            while (FamilyNameSuffixPattern.IsMatch(familyName))
            {
                familyName = FamilyNameSuffixPattern.Replace(familyName, "");
            }
            if (string.IsNullOrEmpty(familyName))
            {
                throw new ArgumentException(
                    "Family name consists solely of a suffix",
                    nameof(familyName)
                );
            }
            return familyName;
        }

        /// <summary>
        /// Returns the provided region code, normalized and formatted.
        /// </summary>
        /// <param name="regionCode">The region code to format.</param>
        /// <exception cref="ArgumentException">If regionCode is invalid.</exception>
        public string FormatRegionCode(string regionCode)
        {
            if (regionCode == null)
            {
                throw new ArgumentNullException(nameof(regionCode), "Null region code");
            }
            regionCode = regionCode.Trim().ToUpper(CultureInfo.InvariantCulture);
            if (string.IsNullOrEmpty(regionCode))
            {
                throw new ArgumentException("Empty or blank region code", nameof(regionCode));
            }
            if (regionCode.Length != 2)
            {
                throw new ArgumentException(
                    $"Region code length is {regionCode.Length}, but " + "length must be 2",
                    nameof(regionCode)
                );
            }
            if (!AllUppercaseCharsPattern.IsMatch(regionCode))
            {
                throw new ArgumentException(
                    "Region code contains characters other than A-Z",
                    nameof(regionCode)
                );
            }
            return regionCode;
        }

        /// <summary>
        /// Returns the provided postal code, normalized and formatted.
        /// </summary>
        /// <param name="postalCode">The postal code to format.</param>
        /// <exception cref="ArgumentException">If postalCode is invalid.</exception>
        public string FormatPostalCode(string postalCode)
        {
            if (postalCode == null)
            {
                throw new ArgumentNullException(nameof(postalCode), "Null postal code");
            }
            postalCode = postalCode.Trim();
            if (string.IsNullOrEmpty(postalCode))
            {
                throw new ArgumentException("Empty or blank postal code", nameof(postalCode));
            }
            return postalCode;
        }

        /// <summary>
        /// Returns the SHA-256 hash of the provided string.
        /// </summary>
        /// <param name="s">The string to hash.</param>
        /// <exception cref="ArgumentException">If the string is null, blank, or empty.</exception>
        public byte[] HashString(string s)
        {
            if (s == null)
            {
                throw new ArgumentNullException(nameof(s), "Null string");
            }
            if (string.IsNullOrWhiteSpace(s))
            {
                throw new ArgumentException("Empty or blank string", nameof(s));
            }
            return _sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(s));
        }

        /// <summary>
        /// Returns the provided byte array, encoded using hex (base 16) encoding.
        /// </summary>
        /// <param name="bytes">The byte array to encode.</param>
        /// <exception cref="ArgumentException">If the byte array is null or empty.</exception>
        public string HexEncode(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes), "Null byte array");
            }
            if (bytes.Length == 0)
            {
                throw new ArgumentException("Empty byte array", nameof(bytes));
            }
            return BitConverter.ToString(bytes).Replace("-", "");
        }

        /// <summary>
        /// Returns the provided byte array, encoded using Base64 encoding.
        /// </summary>
        /// <param name="bytes">The byte array to encode.</param>
        /// <exception cref="ArgumentException">If the byte array is null or empty.</exception>
        public string Base64Encode(byte[] bytes)
        {
            if (bytes == null)
            {
                throw new ArgumentNullException(nameof(bytes), "Null byte array");
            }
            if (bytes.Length == 0)
            {
                throw new ArgumentException("Empty byte array", nameof(bytes));
            }
            return Convert.ToBase64String(bytes);
        }

        /// <summary>
        /// Formats the email address, hashes, and encodes using the specified encoding.
        /// </summary>
        public string ProcessEmailAddress(string email, Encoding encoding)
        {
            return HashAndEncode(FormatEmailAddress(email), encoding);
        }

        /// <summary>
        /// Formats the phone number, hashes, and encodes using the specified encoding.
        /// </summary>
        public string ProcessPhoneNumber(string phoneNumber, Encoding encoding)
        {
            return HashAndEncode(FormatPhoneNumber(phoneNumber), encoding);
        }

        /// <summary>
        /// Formats the given name, hashes, and encodes using the specified encoding.
        /// </summary>
        public string ProcessGivenName(string givenName, Encoding encoding)
        {
            return HashAndEncode(FormatGivenName(givenName), encoding);
        }

        /// <summary>
        /// Formats the family name, hashes, and encodes using the specified encoding.
        /// </summary>
        public string ProcessFamilyName(string familyName, Encoding encoding)
        {
            return HashAndEncode(FormatFamilyName(familyName), encoding);
        }

        /// <summary>
        /// Processes the region code.
        /// </summary>
        public string ProcessRegionCode(string regionCode)
        {
            return FormatRegionCode(regionCode);
        }

        /// <summary>
        /// Processes the postal code.
        /// </summary>
        public string ProcessPostalCode(string postalCode)
        {
            return FormatPostalCode(postalCode);
        }

        private string HashAndEncode(string normalizedString, Encoding encoding)
        {
            byte[] hashBytes = HashString(normalizedString);
            return Encode(hashBytes, encoding);
        }

        private string Encode(byte[] bytes, Encoding encoding)
        {
            switch (encoding)
            {
                case Encoding.Hex:
                    return HexEncode(bytes);
                case Encoding.Base64:
                    return Base64Encode(bytes);
                default:
                    throw new ArgumentException("Invalid encoding: " + encoding);
            }
        }
    }
}
