using System;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace UnityEditor.Localization.Tests
{
    public class PackageTests
    {
        [Test]
        public void ChangelogDatesConformToISO8601()
        {
            const string localizationChangeLog = "Packages/com.unity.localization/CHANGELOG.md";
            Assert.That(File.Exists(localizationChangeLog), Is.True, "Could not find changelog");

            var changelog = File.ReadAllLines(localizationChangeLog);
            var regex = new Regex(@"^##\s+\[.*\]\s+-\s+(?<date>[0-9\-]+)");
            var dateRegex = new Regex(@"(?<year>[0-9][0-9][0-9][0-9])-(?<month>[0-9][0-9])-(?<day>[0-9][0-9])");

            DateTime? lastDate = null;
            foreach (var line in changelog)
            {
                var match = regex.Match(line);
                if (!match.Success)
                    continue;

                var date = match.Groups["date"].Value;
                var dateMatch = dateRegex.Match(date);
                Assert.That(dateMatch.Success, Is.True, $"'{date}' in '{line}' is not in ISO 8601 format");

                Assert.That(int.Parse(dateMatch.Groups["year"].Value), Is.GreaterThanOrEqualTo(2018));
                Assert.That(int.Parse(dateMatch.Groups["month"].Value), Is.GreaterThanOrEqualTo(1).And.LessThanOrEqualTo(12));
                Assert.That(int.Parse(dateMatch.Groups["day"].Value), Is.GreaterThanOrEqualTo(1).And.LessThanOrEqualTo(31));

                // Also ensure dates are ordered.
                var dateTime = DateTime.ParseExact(date, "yyyy-MM-dd", null);
                if (lastDate != null)
                    Assert.That(lastDate.Value, Is.GreaterThan(dateTime));

                lastDate = dateTime;
            }

            Assert.That(lastDate, Is.Not.Null, "Could not find any changelog dates in the changelog file");
        }
    }
}
