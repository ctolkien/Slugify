using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace Slugify
{
    public class SlugHelper : ISlugHelper
    {
        protected Config _config { get; set; }

        public SlugHelper() : this(new Config()) { }

        public SlugHelper(Config config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config), "can't be null use default config or empty constructor.");
        }

        /// <summary>
        /// Implements <see cref="ISlugHelper.GenerateSlug(string)"/>
        /// </summary>
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

            string regex = _config.DeniedCharactersRegex;
            if (regex == null)
            {
                regex = "[^" + Regex.Escape(string.Join("", _config.AllowedChars)).Replace("-", "\\-") + "]";
            }
            inputString = DeleteCharacters(inputString, regex);

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
            // TODO: Implement a source generator so this can be done at compile time :)
            private static readonly char[] s_allowedChars =
                ("abcdefghijklmnopqrstuvwxyz" +
                "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
                "0123456789" +
                "-._").ToCharArray();

            private readonly HashSet<char> _allowedChars = new HashSet<char>(s_allowedChars);

            public Dictionary<string, string> StringReplacements { get; set; } = new Dictionary<string, string>
                {
                    { " ", "-" }
                };

            public bool ForceLowerCase { get; set; } = true;
            public bool CollapseWhiteSpace { get; set; } = true;
            /// <summary>
            /// Note: Setting this property will stop the AllowedChars feature from being used
            /// </summary>
            public string DeniedCharactersRegex { get; set; }
            public HashSet<char> AllowedChars
            {
                get
                {
                    return DeniedCharactersRegex == null ? _allowedChars : throw new InvalidOperationException("After setting DeniedCharactersRegex the AllowedChars feature cannot be used.");
                }
            }
            public bool CollapseDashes { get; set; } = true;
            public bool TrimWhitespace { get; set; } = true;
        }
    }
}

