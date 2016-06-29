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

        public SlugHelper() :
            this(new SlugHelper.Config())
        {
        }

        public SlugHelper(Config config)
        {
            if (config != null)
                _config = config;
            else
                throw new ArgumentNullException(nameof(config), "can't be null use default config or empty construct.");
        }

        public string GenerateSlug(string str)
        {
            if (_config.ForceLowerCase)
                str = str.ToLower();

            str = CleanWhiteSpace(str, _config.CollapseWhiteSpace);
            str = ApplyReplacements(str, _config.CharacterReplacements);
            str = RemoveDiacritics(str);
            str = DeleteCharacters(str, _config.DeniedCharactersRegex);

            return str;
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

            return (sb.ToString().Normalize(NormalizationForm.FormC));
        }

        protected string ApplyReplacements(string str, Dictionary<string, string> replacements)
        {
            var sb = new StringBuilder(str);

            foreach (KeyValuePair<string, string> replacement in replacements)
                sb.Replace(replacement.Key, replacement.Value);

            return sb.ToString();
        }

        protected string DeleteCharacters(string str, string regex)
        {
            return Regex.Replace(str, regex, "");
        }

        public class Config
        {
            public Dictionary<string, string> CharacterReplacements { get; set; }
            public bool ForceLowerCase { get; set; }
            public bool CollapseWhiteSpace { get; set; }
            public string DeniedCharactersRegex { get; set; }

            public Config()
            {
                CharacterReplacements = new Dictionary<string, string>();
                CharacterReplacements.Add(" ", "-");

                ForceLowerCase = true;
                CollapseWhiteSpace = true;
                DeniedCharactersRegex = @"[^a-zA-Z0-9\-\._]";
            }
        }

    }

}

