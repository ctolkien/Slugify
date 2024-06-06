using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Slugify;

public class SlugHelper(SlugHelperConfiguration config) : ISlugHelper
{
    protected static readonly SlugHelperConfiguration _defaultConfig = new();
    protected static readonly Dictionary<string, Regex> _deleteRegexMap = [];
    public SlugHelperConfiguration Config { get; set; } = config ?? throw new ArgumentNullException(nameof(config), "can't be null use default config or empty constructor.");

    public SlugHelper() : this(_defaultConfig) { }

    public virtual string GenerateSlug(string inputString)
    {
        var normalizedInput = inputString.Normalize(NormalizationForm.FormD);

        normalizedInput = Config.TrimWhitespace ? normalizedInput.Trim() : normalizedInput;
        normalizedInput = Config.ForceLowerCase ? normalizedInput.ToLower() : normalizedInput;

        var sb = new StringBuilder(normalizedInput);

        foreach (var replacement in Config.StringReplacements)
        {
            var search = replacement.Key.Normalize(NormalizationForm.FormD);
            var replace = replacement.Value.Normalize(NormalizationForm.FormD);

            sb.Replace(search, replace);
        }

        if (Config.DeniedCharactersRegex == null)
        {
            var allowedChars = Config.AllowedChars;
            for (int i = 0; i < sb.Length;)
            {
                if (!allowedChars.Contains(sb[i]))
                {
                    sb.Remove(i, 1);
                }
                else
                {
                    i++;
                }
            }
        }
        else // Back compat regex
        {
            var currentValue = sb.ToString();
            sb.Clear();
            sb.Insert(0, Config.DeniedCharactersRegex.Replace(currentValue, string.Empty));
        }

        if (Config.CollapseDashes)
        {
            for (int i = 0; i < sb.Length - 1;)
            {
                if (sb[i] == '-' && sb[i + 1] == '-')
                {
                    sb.Remove(i, 1);
                }
                else
                {
                    i++;
                }
            }
        }
        return sb.ToString();
    }

}

