using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Slugify
{
    public class SlugHelper
    {
        protected Config _config { get; set; }

        public SlugHelper() : this(new Config()) { }

        public SlugHelper(Config config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config), "can't be null use default config or empty constructor.");
        }

        /// <summary>
        /// Generates a slug from the provided <paramref name="inputString"/>
        /// </summary>
        /// <param name="inputString">The string to slugify</param>
        /// <returns>A slugified version of <paramref name="inputString"/></returns>
        public string GenerateSlug(string inputString)
        {
            if (_config.ForceLowerCase)
            {
                inputString = inputString.ToLower();
            }

            if (_config.TrimWhitespace)
            {
                inputString = inputString.Trim();
            }

            inputString = CleanWhiteSpace(inputString, _config.CollapseWhiteSpace);
            inputString = ApplyReplacements(inputString, _config.StringReplacements);
            inputString = RemoveDiacritics(inputString);
            inputString = DeleteCharacters(inputString, _config.DeniedCharactersRegex);

            if (_config.CollapseDashes)
            {
                inputString = Regex.Replace(inputString, "--+", "-");
            }

            return inputString;
        }

        protected string CleanWhiteSpace(string str, bool collapse)
        {
            return Regex.Replace(str, collapse ? @"\s+" : @"\s", " ");
        }

        // Thanks http://stackoverflow.com/a/249126!
        protected string RemoveDiacritics(string str)
        {
            var stFormD = str.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();

            for (int ich = 0; ich < stFormD.Length; ich++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD[ich]);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(stFormD[ich]);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        protected string ApplyReplacements(string str, Dictionary<string, string> replacements)
        {
            var sb = new StringBuilder(str);

            foreach (KeyValuePair<string, string> replacement in replacements)
            {
                sb = sb.Replace(replacement.Key, replacement.Value);
            }

            return sb.ToString();
        }

        protected string DeleteCharacters(string str, string regex)
        {
            return Regex.Replace(str, regex, "");
        }

        /// <summary>
        /// Used to configure the a <see cref="SlugHelper"/> instance
        /// </summary>
        public class Config
        {
            public Config()
            {
                StringReplacements = new Dictionary<string, string>
                {
                    { " ", "-" }
                };
            }

            public Dictionary<string, string> StringReplacements { get; set; }
            public bool ForceLowerCase { get; set; } = true;
            public bool CollapseWhiteSpace { get; set; } = true;
            public string DeniedCharactersRegex { get; set; } = @"[^a-zA-Z0-9\-\._]";
            public bool CollapseDashes { get; set; } = true;
            public bool TrimWhitespace { get; set; } = true;
        }
    }
}

