using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Slugify
{
    public class SlugHelperImproved : ISlugHelper
    {
        private static readonly Lazy<Regex> _collapseDashesRegex = new Lazy<Regex>(() => new Regex("--+", RegexOptions.Compiled));
        private static readonly Dictionary<string, Regex> _deleteRegexMap = new Dictionary<string, Regex>();
        private static readonly Lazy<SlugHelper.Config> _defaultConfig = new Lazy<SlugHelper.Config>(() => new SlugHelper.Config());

        protected SlugHelper.Config _config { get; set; }

        private readonly Regex _deniedCharactersRegex;

        public SlugHelperImproved() : this(_defaultConfig.Value) { }

        public SlugHelperImproved(SlugHelper.Config config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config), "can't be null use default config or empty constructor.");

            if (!_deleteRegexMap.TryGetValue(_config.DeniedCharactersRegex, out _deniedCharactersRegex))
            {
                _deniedCharactersRegex = new Regex(_config.DeniedCharactersRegex, RegexOptions.Compiled);
                _deleteRegexMap.Add(_config.DeniedCharactersRegex, _deniedCharactersRegex);
            }
        }

        /// <summary>
        /// Implements <see cref="ISlugHelper.GenerateSlug(string)"/>
        /// </summary>
        public string GenerateSlug(string inputString)
        {
            StringBuilder sb = new StringBuilder();

            // First we trim and lowercase if necessary
            PrepareStringBuilder(inputString.Normalize(NormalizationForm.FormD), sb);
            ApplyStringReplacements(sb);
            RemoveNonSpacingMarks(sb);

            inputString = sb.ToString();

            inputString = DeleteCharacters(inputString);

            if (_config.CollapseDashes)
            {
                inputString = _collapseDashesRegex.Value.Replace(inputString, "-");
            }

            return inputString;
        }

        private void PrepareStringBuilder(string inputString, StringBuilder sb)
        {
            bool seenFirstNonWhitespace = false;
            int indexOfLastNonWhitespace = 0;
            for (int i = 0; i < inputString.Length; i++)
            {
                // first, clean whitepace
                char c = inputString[i];
                if (!seenFirstNonWhitespace && char.IsWhiteSpace(c))
                {
                    if (_config.TrimWhitespace)
                    {
                        continue;
                    }
                    else
                    {
                        sb.Append(c);
                    }
                }
                else
                {
                    seenFirstNonWhitespace = true;
                    if (!char.IsWhiteSpace(c))
                    {
                        indexOfLastNonWhitespace = sb.Length;
                    }
                    else
                    {
                        c = ' ';

                        if (_config.CollapseWhiteSpace)
                        {
                            while ((i + 1) < inputString.Length && char.IsWhiteSpace(inputString[i + 1]))
                            {
                                i++;
                            }
                        }
                    }
                    if (_config.ForceLowerCase)
                    {
                        c = char.ToLower(c);
                    }

                    sb.Append(c);
                }
            }

            if (_config.TrimWhitespace)
            {
                sb.Length = indexOfLastNonWhitespace + 1;
            }
        }

        private void ApplyStringReplacements(StringBuilder sb)
        {
            foreach (var replacement in _config.StringReplacements)
            {
                for (int i = 0; i < sb.Length; i++)
                {
                    if (SubstringEquals(sb, i, replacement.Key))
                    {
                        sb.Remove(i, replacement.Key.Length);
                        sb.Insert(i, replacement.Value);

                        i += replacement.Value.Length - 1;
                    }
                }
            }
        }

        private static bool SubstringEquals(StringBuilder sb, int index, string toMatch)
        {
            if (sb.Length - index < toMatch.Length)
            {
                return false;
            }

            for (int i = index; i < sb.Length; i++)
            {
                int matchIndex = i - index;

                if (matchIndex == toMatch.Length)
                {
                    return true;
                }
                else if (sb[i] != toMatch[matchIndex])
                {
                    return false;
                }
            }
            return (sb.Length - index) == toMatch.Length;
        }

        // Thanks http://stackoverflow.com/a/249126!
        protected void RemoveNonSpacingMarks(StringBuilder sb)
        {
            for (var ich = 0; ich < sb.Length; ich++)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(sb[ich]) == UnicodeCategory.NonSpacingMark)
                {
                    sb.Remove(ich, 1);
                    ich--;
                }
            }
        }

        protected string DeleteCharacters(string str)
        {
            return _deniedCharactersRegex.Replace(str, string.Empty);
        }
    }
}

