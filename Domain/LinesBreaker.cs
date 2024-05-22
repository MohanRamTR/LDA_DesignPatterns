using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LDA_DesignPatterns_Chap1.Domain
{
    public class LinesBreaker
    {
        private IEnumerable<(string separatorPattern, string appendLeft, string prependRight)[]> Rules { get; } = new[]
        {
            new[]
            {
                (", ", "...", "... "),
                ("; ", "...", "... "),
                (" - ", "...", "... "),
            },
            new[]
            {
                (" and ", "...", "... and "),
                (" or ", "...", "... or "),
            },
            new[]
            {
                (" to ", "...", "... to "),
                (" then ", "...", "... then "),
            },
            new[]
            {
                (" ", "...", "... ")
            },
        };

        public IEnumerable<string> BreakLongLines(IEnumerable<string> text, int maxLineCharacters, int minBrokenLength) =>
            text.SelectMany(line => BreakLongLine(line, maxLineCharacters, minBrokenLength));

        public IEnumerable<string> BreakLongLine(string line, int maxLength, int minBrokenLength)
        {
            string remaining = line;

            while (!string.IsNullOrEmpty(remaining))
            {
                if (remaining.Length <= maxLength)
                {
                    yield return remaining;
                    break;
                }

                bool broken = false;
                foreach (var rules in Rules)
                {
                    var split = TryBreakLongLine(remaining, rules, maxLength, minBrokenLength).ToList();

                    if (split.Any())
                    {
                        var (left, right) = split.First();
                        yield return left;
                        remaining = right;
                        broken = true;
                        break;
                    }
                }

                if (!broken)
                {
                    yield return remaining;
                    break;
                }
            }
        }

        private IEnumerable<(string left, string right)> TryBreakLongLine(string line,
            IEnumerable<(string separatorPattern, string appendLeft, string prependRight)> rules,
            int maxLength, int minBrokenLength) =>
            rules.SelectMany(rule => BreakLongLine(line, rule, maxLength, minBrokenLength))
                .OrderBy(split => maxLength - split.left.Length);

        private IEnumerable<(string left, string right)> BreakLongLine(string line,
            (string separatorPattern, string appendLeft, string prependRight) rule,
            int maxLength, int minBrokenLength)
        {
            var regex = new Regex(rule.separatorPattern);
            var matches = regex.Matches(line);

            foreach (Match match in matches)
            {
                var split = (
                    left: line.Substring(0, match.Index) + rule.appendLeft,
                    right: rule.prependRight + line[(match.Index + match.Length)..]);

                if (minBrokenLength <= split.left.Length && split.left.Length <= maxLength)
                {
                    yield return split;
                }
            }
        }
    }
}
