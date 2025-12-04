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

using NUnit.Framework;

namespace Google.Ads.DataManager.Util.Tests
{
    [TestFixture]
    public class UserDataFormatterTests
    {
        private readonly UserDataFormatter _formatter = new UserDataFormatter();

        [Test]
        public void TestFormatEmailAddress_ValidInputs()
        {
            Assert.That(
                _formatter.FormatEmailAddress("QuinnY@example.com"),
                Is.EqualTo("quinny@example.com")
            );
            Assert.That(
                _formatter.FormatEmailAddress("QuinnY@EXAMPLE.com"),
                Is.EqualTo("quinny@example.com")
            );
        }

        [Test]
        public void TestFormatEmailAddress_InvalidInput_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _formatter.FormatEmailAddress(null!));
            Assert.Throws<ArgumentException>(() => _formatter.FormatEmailAddress("@example.com"));
            Assert.Throws<ArgumentException>(() => _formatter.FormatEmailAddress("quinn"));
        }

        [Test]
        public void TestFormatEmailAddress_GmailVariations()
        {
            Assert.That(
                _formatter.FormatEmailAddress("jefferson.Loves.hiking@gmail.com"),
                Is.EqualTo("jeffersonloveshiking@gmail.com")
            );
            Assert.That(
                _formatter.FormatEmailAddress("j.e.f..ferson.Loves.hiking@gmail.com"),
                Is.EqualTo("jeffersonloveshiking@gmail.com")
            );
            Assert.That(
                _formatter.FormatEmailAddress("jefferson.Loves.hiking@googlemail.com"),
                Is.EqualTo("jeffersonloveshiking@googlemail.com")
            );
            Assert.That(
                _formatter.FormatEmailAddress("j.e.f..ferson.Loves.hiking@googlemail.com"),
                Is.EqualTo("jeffersonloveshiking@googlemail.com")
            );
        }

        [Test]
        public void TestFormatPhoneNumber_ValidInputs()
        {
            string[,] validInputsOutputs =
            {
                { "1 800 555 0100", "+18005550100" },
                { "18005550100", "+18005550100" },
                { "+1 800-555-0100", "+18005550100" },
                { "441134960987", "+441134960987" },
                { "+441134960987", "+441134960987" },
                { "+44-113-496-0987", "+441134960987" },
            };
            for (int i = 0; i < validInputsOutputs.GetLength(0); i++)
            {
                Assert.That(
                    _formatter.FormatPhoneNumber(validInputsOutputs[i, 0]),
                    Is.EqualTo(validInputsOutputs[i, 1])
                );
            }
        }

        [Test]
        public void TestFormatPhoneNumber_InvalidInput_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _formatter.FormatPhoneNumber(null!));
            Assert.Throws<ArgumentException>(() => _formatter.FormatPhoneNumber("+abc-DEF"));
            Assert.Throws<ArgumentException>(() => _formatter.FormatPhoneNumber("++++"));
        }

        [Test]
        public void TestFormatGivenName_ValidInputs()
        {
            Assert.That(_formatter.FormatGivenName(" Alex   "), Is.EqualTo("alex"));
            Assert.That(_formatter.FormatGivenName(" Mr. Alex   "), Is.EqualTo("alex"));
            Assert.That(_formatter.FormatGivenName(" Mrs. Alex   "), Is.EqualTo("alex"));
            Assert.That(_formatter.FormatGivenName(" Dr. Alex   "), Is.EqualTo("alex"));
            Assert.That(_formatter.FormatGivenName(" Alex Dr."), Is.EqualTo("alex"));
            Assert.That(_formatter.FormatGivenName(" Mralex   "), Is.EqualTo("mralex"));
        }

        [Test]
        public void TestFormatGivenName_InvalidInput_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _formatter.FormatGivenName(null!));
            Assert.Throws<ArgumentException>(() => _formatter.FormatGivenName(" "));
            Assert.Throws<ArgumentException>(() => _formatter.FormatGivenName(" Mr. "));
        }

        [Test]
        public void TestFormatFamilyName_ValidInputs()
        {
            Assert.That(_formatter.FormatFamilyName(" Quinn   "), Is.EqualTo("quinn"));
            Assert.That(_formatter.FormatFamilyName("Quinn-Alex"), Is.EqualTo("quinn-alex"));
            Assert.That(_formatter.FormatFamilyName(" Quinn, Jr.   "), Is.EqualTo("quinn"));
            Assert.That(_formatter.FormatFamilyName(" Quinn,Jr.   "), Is.EqualTo("quinn"));
            Assert.That(_formatter.FormatFamilyName(" Quinn Sr.  "), Is.EqualTo("quinn"));
            Assert.That(_formatter.FormatFamilyName("quinn, jr. dds"), Is.EqualTo("quinn"));
            Assert.That(_formatter.FormatFamilyName("quinn, jr., dds"), Is.EqualTo("quinn"));
            Assert.That(_formatter.FormatFamilyName("Boardds"), Is.EqualTo("boardds"));
            Assert.That(_formatter.FormatFamilyName("lacparm"), Is.EqualTo("lacparm"));
        }

        [Test]
        public void TestFormatFamilyName_InvalidInput_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _formatter.FormatFamilyName(null!));
            Assert.Throws<ArgumentException>(() => _formatter.FormatFamilyName(" "));
            Assert.Throws<ArgumentException>(() => _formatter.FormatFamilyName(", Jr. "));
            Assert.Throws<ArgumentException>(() => _formatter.FormatFamilyName(",Jr.,DDS "));
        }

        [Test]
        public void TestFormatRegionCode_ValidInputs()
        {
            Assert.That(_formatter.FormatRegionCode("us"), Is.EqualTo("US"));
            Assert.That(_formatter.FormatRegionCode("us  "), Is.EqualTo("US"));
            Assert.That(_formatter.FormatRegionCode("  us  "), Is.EqualTo("US"));
        }

        [Test]
        public void TestFormatRegionCode_InvalidInputs()
        {
            Assert.Throws<ArgumentNullException>(() => _formatter.FormatRegionCode(null!));
            Assert.Throws<ArgumentException>(() => _formatter.FormatRegionCode(""));
            Assert.Throws<ArgumentException>(() => _formatter.FormatRegionCode("  "));
            Assert.Throws<ArgumentException>(() => _formatter.FormatRegionCode("u"));
            Assert.Throws<ArgumentException>(() => _formatter.FormatRegionCode(" usa "));
            Assert.Throws<ArgumentException>(() => _formatter.FormatRegionCode(" u s "));
            Assert.Throws<ArgumentException>(() => _formatter.FormatRegionCode(" u2 "));
        }

        [Test]
        public void TestFormatPostalCode_ValidInputs()
        {
            Assert.That(_formatter.FormatPostalCode("94045"), Is.EqualTo("94045"));
            Assert.That(_formatter.FormatPostalCode(" 94045  "), Is.EqualTo("94045"));
            Assert.That(_formatter.FormatPostalCode("1229-076"), Is.EqualTo("1229-076"));
            Assert.That(_formatter.FormatPostalCode("  1229-076  "), Is.EqualTo("1229-076"));
        }

        [Test]
        public void TestFormatPostalCode_InvalidInputs()
        {
            Assert.Throws<ArgumentNullException>(() => _formatter.FormatPostalCode(null!));
            Assert.Throws<ArgumentException>(() => _formatter.FormatPostalCode(""));
            Assert.Throws<ArgumentException>(() => _formatter.FormatPostalCode("  "));
        }

        [Test]
        public void TestHashString_ValidInputs()
        {
            Func<string, string> hashAndEncode = s =>
                BitConverter.ToString(_formatter.HashString(s)).Replace("-", "");
            Assert.That(
                hashAndEncode("alexz@example.com"),
                Is.EqualTo(
                    "509E933019BB285A134A9334B8BB679DFF79D0CE023D529AF4BD744D47B4FD8A"
                ).IgnoreCase
            );
            Assert.That(
                hashAndEncode("+18005550100"),
                Is.EqualTo(
                    "FB4F73A6EC5FDB7077D564CDD22C3554B43CE49168550C3B12C547B78C517B30"
                ).IgnoreCase
            );
        }

        [Test]
        public void TestHashString_InvalidInput_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _formatter.HashString(null!));
            Assert.Throws<ArgumentException>(() => _formatter.HashString(""));
            Assert.Throws<ArgumentException>(() => _formatter.HashString(" "));
            Assert.Throws<ArgumentException>(() => _formatter.HashString("  "));
        }

        [Test]
        public void TestHexEncode_ValidInputs()
        {
            // Compares to expected values, ignoring case. Hex values are not case-sensitive.
            Assert.That(
                _formatter.HexEncode(System.Text.Encoding.UTF8.GetBytes("acK123")),
                Is.EqualTo("61634b313233").IgnoreCase
            );
            Assert.That(
                _formatter.HexEncode(System.Text.Encoding.UTF8.GetBytes("999_XYZ")),
                Is.EqualTo("3939395f58595a").IgnoreCase
            );
        }

        [Test]
        public void TestHexEncode_InvalidInput_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _formatter.HexEncode(null!));
            Assert.Throws<ArgumentException>(() => _formatter.HexEncode(new byte[0]));
        }

        [Test]
        public void TestBase64Encode_InvalidInput_ThrowsException()
        {
            Assert.Throws<ArgumentNullException>(() => _formatter.Base64Encode(null!));
            Assert.Throws<ArgumentException>(() => _formatter.Base64Encode(new byte[0]));
        }

        [Test]
        public void TestBase64Encode_ValidInputs()
        {
            // Compares to expected values. Base64 values are case-sensitive.
            Assert.That(
                _formatter.Base64Encode(System.Text.Encoding.UTF8.GetBytes("acK123")),
                Is.EqualTo("YWNLMTIz")
            );
            Assert.That(
                _formatter.Base64Encode(System.Text.Encoding.UTF8.GetBytes("999_XYZ")),
                Is.EqualTo("OTk5X1hZWg==")
            );
        }

        [Test]
        public void TestProcessEmailAddress_ValidInputs_HexEncoding()
        {
            const string encodedHash =
                "509e933019bb285a134a9334b8bb679dff79d0ce023d529af4bd744d47b4fd8a";
            string[] variants =
            {
                "alexz@example.com",
                "  alexz@example.com",
                "  alexz@example.com",
                "  ALEXZ@example.com   ",
                "  alexz@EXAMPLE.com   ",
            };
            foreach (var emailVariant in variants)
            {
                // Compares to expected values, ignoring case. Hex values are not case-sensitive.
                Assert.That(
                    _formatter.ProcessEmailAddress(emailVariant, UserDataFormatter.Encoding.Hex),
                    Is.EqualTo(encodedHash).IgnoreCase
                );
            }
        }

        [Test]
        public void TestProcessEmailAddress_ValidInputs_Base64Encoding()
        {
            const string encodedHash = "UJ6TMBm7KFoTSpM0uLtnnf950M4CPVKa9L10TUe0/Yo=";
            string[] variants =
            {
                "alexz@example.com",
                "  alexz@example.com",
                "  alexz@example.com",
                "  ALEXZ@example.com   ",
                "  alexz@EXAMPLE.com   ",
            };
            foreach (var emailVariant in variants)
            {
                // Compares to expected values. Base64 values are case-sensitive.
                Assert.That(
                    _formatter.ProcessEmailAddress(emailVariant, UserDataFormatter.Encoding.Base64),
                    Is.EqualTo(encodedHash)
                );
            }
        }

        [Test]
        public void TestProcessPhoneNumber_ValidInputs_HexEncoding()
        {
            const string encodedHash =
                "fb4f73a6ec5fdb7077d564cdd22c3554b43ce49168550c3b12c547b78c517b30";
            // Compares to expected values, ignoring case. Hex values are not case-sensitive.
            Assert.That(
                _formatter.ProcessPhoneNumber("+18005550100", UserDataFormatter.Encoding.Hex),
                Is.EqualTo(encodedHash).IgnoreCase
            );
            Assert.That(
                _formatter.ProcessPhoneNumber("   +1-800-555-0100", UserDataFormatter.Encoding.Hex),
                Is.EqualTo(encodedHash).IgnoreCase
            );
            Assert.That(
                _formatter.ProcessPhoneNumber("1-800-555-0100   ", UserDataFormatter.Encoding.Hex),
                Is.EqualTo(encodedHash).IgnoreCase
            );
        }

        [Test]
        public void TestProcessPhoneNumber_ValidInputs_Base64Encoding()
        {
            const string encodedHash = "+09zpuxf23B31WTN0iw1VLQ85JFoVQw7EsVHt4xRezA=";
            // Compares to expected values. Base64 values are case-sensitive.
            Assert.That(
                _formatter.ProcessPhoneNumber("+18005550100", UserDataFormatter.Encoding.Base64),
                Is.EqualTo(encodedHash)
            );
            Assert.That(
                _formatter.ProcessPhoneNumber(
                    "   +1-800-555-0100",
                    UserDataFormatter.Encoding.Base64
                ),
                Is.EqualTo(encodedHash)
            );
            Assert.That(
                _formatter.ProcessPhoneNumber(
                    "1-800-555-0100   ",
                    UserDataFormatter.Encoding.Base64
                ),
                Is.EqualTo(encodedHash)
            );
        }

        [Test]
        public void TestProcessGivenName_ValidInputs_HexEncoding()
        {
            const string encodedHash =
                "128A07BFE2DF877C52076E60D7774CF5BAAA046C5A6C48DAF30FF43ECCA2F814";
            // Compares to expected values, ignoring case. Hex values are not case-sensitive.
            Assert.That(
                _formatter.ProcessGivenName("Givenname", UserDataFormatter.Encoding.Hex),
                Is.EqualTo(encodedHash).IgnoreCase
            );
            Assert.That(
                _formatter.ProcessGivenName("  GivenName  ", UserDataFormatter.Encoding.Hex),
                Is.EqualTo(encodedHash).IgnoreCase
            );
        }

        [Test]
        public void TestProcessGivenName_ValidInputs_Base64Encoding()
        {
            // Compares to expected values. Base64 values are case-sensitive.
            const string encodedHash = "EooHv+Lfh3xSB25g13dM9bqqBGxabEja8w/0Psyi+BQ=";
            Assert.That(
                _formatter.ProcessGivenName("Givenname", UserDataFormatter.Encoding.Base64),
                Is.EqualTo(encodedHash)
            );
            Assert.That(
                _formatter.ProcessGivenName("  GivenName  ", UserDataFormatter.Encoding.Base64),
                Is.EqualTo(encodedHash)
            );
        }

        [Test]
        public void TestProcessFamilyName_ValidInputs_HexEncoding()
        {
            const string encodedHash =
                "77762c287e61ce065bee5c15464012c6fbe088398b8057627d5577249430d574";
            // Compares to expected values, ignoring case. Hex values are not case-sensitive.
            Assert.That(
                _formatter.ProcessFamilyName("Familyname", UserDataFormatter.Encoding.Hex),
                Is.EqualTo(encodedHash).IgnoreCase
            );
            Assert.That(
                _formatter.ProcessFamilyName("  FamilyName ", UserDataFormatter.Encoding.Hex),
                Is.EqualTo(encodedHash).IgnoreCase
            );
        }

        [Test]
        public void TestProcessFamilyName_ValidInputs_Base64Encoding()
        {
            const string encodedHash = "d3YsKH5hzgZb7lwVRkASxvvgiDmLgFdifVV3JJQw1XQ=";
            // Compares to expected values. Base64 values are case-sensitive.
            Assert.That(
                _formatter.ProcessFamilyName("Familyname", UserDataFormatter.Encoding.Base64),
                Is.EqualTo(encodedHash)
            );
            Assert.That(
                _formatter.ProcessFamilyName("  FamilyName ", UserDataFormatter.Encoding.Base64),
                Is.EqualTo(encodedHash)
            );
        }

        [Test]
        public void TestProcessRegionCode_ValidInputs()
        {
            Assert.That(_formatter.ProcessRegionCode(" us"), Is.EqualTo("US"));
            Assert.That(_formatter.ProcessRegionCode(" uS "), Is.EqualTo("US"));
        }

        [Test]
        public void TestProcessPostalCode_ValidInputs()
        {
            Assert.That(_formatter.ProcessPostalCode("1229-076"), Is.EqualTo("1229-076"));
            Assert.That(_formatter.ProcessPostalCode(" 1229-076  "), Is.EqualTo("1229-076"));
        }
    }
}
