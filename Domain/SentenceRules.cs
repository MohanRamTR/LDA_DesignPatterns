using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace LDA_DesignPatterns_Chap1.Domain
{
    public class SentenceRules
    {
        private IEnumerable<(string pattern, string extract, string remove)> Rules { get; } = new[]
        {
            (@"^(?<remove>(?<extract>(\.\.\.|[^\.])+)\.)$", "${extract}", "${remove}"),
            (@"^(?<remove>(?<extract>[^\.]+),)$", "${extract}", "${remove}"),
            (@"^(?<remove>(?<extract>(\.\.\.|[^\.])+)\.)[^\.].*$", "${extract}", "${remove}"),
            (@"^(?<remove>(?<extract>[^:]+):).*$", "${extract}", "${remove}"),
            (@"^(?<extract>.+\?).*$", "${extract}", "${extract}"),
            (@"^(?<extract>.+\!).*$", "${extract}", "${extract}"),
        };

        public IEnumerable<string> Split(IEnumerable<string> text) =>
            text.SelectMany(BreakSentences);

        private IEnumerable<string> BreakSentences(string text)
        {
            string remaining = text.Trim();
            while (remaining.Length > 0)
            {
                var extractionResult = FindShortestExtractionRule(Rules, remaining).FirstOrDefault();
                if (extractionResult != default)
                {
                    var (extracted, removedLength) = extractionResult;
                    var rest = remaining.Substring(removedLength).Trim();
                    yield return extracted;
                    remaining = rest;
                }
                else
                {
                    yield return remaining;
                    yield break;
                }
            }
        }

        private IEnumerable<(string extracted, int removedLength)> FindShortestExtractionRule(
            IEnumerable<(string pattern, string extractPattern, string removePattern)> rules,
            string text)
        {
            var extractionResults = rules
                .Select(rule => (
                    pattern: new Regex(rule.pattern),
                    rule.extractPattern,
                    rule.removePattern))
                .Select(rule => (
                    rule.pattern,
                    match: rule.pattern.Match(text),
                    rule.extractPattern,
                    rule.removePattern))
                .Where(rule => rule.match.Success)
                .Select(rule => (
                    extracted: rule.pattern.Replace(text, rule.extractPattern),
                    remove: rule.pattern.Replace(text, rule.removePattern)))
                .Select(tuple => (
                    tuple.extracted,
                    removedLength: text.Length - tuple.remove.Length))
                .OrderBy(tuple => tuple.removedLength);

            return extractionResults;
        }
    }
}
