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
        private readonly Regex _cleanWhiteSpaceRegex;

        public SlugHelperImproved() : this(_defaultConfig.Value) { }

        public SlugHelperImproved(SlugHelper.Config config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config), "can't be null use default config or empty constructor.");

            if (!_deleteRegexMap.TryGetValue(_config.DeniedCharactersRegex, out _deniedCharactersRegex))
            {
                _deniedCharactersRegex = new Regex(_config.DeniedCharactersRegex, RegexOptions.Compiled);
                _deleteRegexMap.Add(_config.DeniedCharactersRegex, _deniedCharactersRegex);
            }

            _cleanWhiteSpaceRegex = new Regex(_config.CollapseWhiteSpace ? @"\s+" : @"\s", RegexOptions.Compiled);
        }

        /// <summary>
        /// Implements <see cref="ISlugHelper.GenerateSlug(string)"/>
        /// </summary>
        public string GenerateSlug(string inputString)
        {
            if (_config.TrimWhitespace)
            {
                inputString = inputString.Trim();
            }

            if (_config.ForceLowerCase)
            {
                inputString = inputString.ToLower();
            }

            inputString = CleanWhiteSpace(inputString);
            inputString = ApplyReplacements(inputString);
            inputString = RemoveDiacritics(inputString);
            inputString = DeleteCharacters(inputString);

            if (_config.CollapseDashes)
            {
                inputString = _collapseDashesRegex.Value.Replace(inputString, "-");
            }

            return inputString;
        }

        protected string CleanWhiteSpace(string str)
        {
            return _cleanWhiteSpaceRegex.Replace(str, " ");
        }

        // Thanks http://stackoverflow.com/a/249126!
        protected string RemoveDiacritics(string str)
        {
            var stFormD = str.Normalize(NormalizationForm.FormD);

            //perf: initialise this with the length of the chars
            var sb = new StringBuilder(stFormD.Length);

            for (var ich = 0; ich < stFormD.Length; ich++)
            {
                var uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        protected string ApplyReplacements(string str)
        {
            //perf: don't use string builder here, it's faster without
            foreach (var replacement in _config.StringReplacements)
            {
                str = str.Replace(replacement.Key, replacement.Value);
            }

            return str;
        }

        protected string DeleteCharacters(string str)
        {
            return _deniedCharactersRegex.Replace(str, string.Empty);
        }
    }
}

