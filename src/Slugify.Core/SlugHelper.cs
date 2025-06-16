using Slugify.Core;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Slugify;

/// <summary>
/// Generates a URL-friendly slug from a given string by normalizing and replacing characters.
/// </summary>
/// <param name="config">Specifies the configuration options for generating the slug, including transformations and allowed characters.</param>
public class SlugHelper(SlugHelperConfiguration config) : ISlugHelper
{
    protected static readonly SlugHelperConfiguration _defaultConfig = new();
    protected static readonly Dictionary<string, Regex> _deleteRegexMap = [];
    public SlugHelperConfiguration Config { get; set; } = config ?? throw new ArgumentNullException(nameof(config), "can't be null use default config or empty constructor.");

    public SlugHelper() : this(_defaultConfig) { }

    /// <summary>
    /// Generates a URL-friendly slug from the provided string by normalizing and replacing characters.
    /// </summary>
    /// <param name="inputString">The string to be transformed into a slug format.</param>
    /// <returns>A string that represents the slug version of the input, with specified transformations applied.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the input string is null.</exception>
    public virtual string GenerateSlug(string inputString)
    {
        if (inputString is null)
        {
            throw new ArgumentNullException(nameof(inputString));
        }

        var normalizedInput = config.SupportNonAsciiLanguages
            ? UnicodeDecoder.UniDecode(inputString)
            : inputString;

        normalizedInput = normalizedInput.Normalize(NormalizationForm.FormD);
        normalizedInput = Config.TrimWhitespace ? normalizedInput.Trim() : normalizedInput;
        normalizedInput = Config.ForceLowerCase ? normalizedInput.ToLowerInvariant() : normalizedInput;

        var sb = new StringBuilder(normalizedInput);

        foreach (var replacement in Config.StringReplacements)
        {
            var search = replacement.Key.Normalize(NormalizationForm.FormD);
            var replace = replacement.Value.Normalize(NormalizationForm.FormD);

            sb.Replace(search, replace);
        }

        if (Config.DeniedCharactersRegex == null)
        {
            var allowedChars = Config.AllowedCharacters;
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

        if (Config.MaximumLength.HasValue && sb.Length > Config.MaximumLength.Value)
        {
            if (Config.EnableHashedShortening)
            {
                // Generate hash from the full slug before truncation
                var fullSlug = sb.ToString();
                var hash = GenerateSlugHash(fullSlug);
                
                // Calculate target length leaving room for hash (2 chars) and separator (1 char)
                var targetLength = Config.MaximumLength.Value - 3; // -3 for "-XX" pattern
                if (targetLength < 1)
                {
                    // If maximum length is too small for hash postfix, just truncate normally
                    sb.Remove(Config.MaximumLength.Value, sb.Length - Config.MaximumLength.Value);
                }
                else
                {
                    // Truncate to make room for hash
                    sb.Remove(targetLength, sb.Length - targetLength);
                    
                    // Remove trailing dash if it exists
                    while (sb.Length > 0 && sb[sb.Length - 1] == '-')
                    {
                        sb.Remove(sb.Length - 1, 1);
                    }
                    
                    // Append hash postfix
                    sb.Append('-');
                    sb.Append(hash);
                }
            }
            else
            {
                // Original behavior: simple truncation
                sb.Remove(Config.MaximumLength.Value, sb.Length - Config.MaximumLength.Value);
                // Remove trailing dash if it exists
                if (sb.Length > 0 && sb[sb.Length - 1] == '-')
                {
                    sb.Remove(sb.Length - 1, 1);
                }
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Generates a short 2-character hash from the input string for use as a postfix.
    /// </summary>
    /// <param name="input">The input string to hash</param>
    /// <returns>A 2-character lowercase hexadecimal hash</returns>
    private static string GenerateSlugHash(string input)
    {
        // Use a simple hash based on the string's hash code
        // This provides consistent results across runs while being simple
        var hash = input.GetHashCode();
        
        // Convert to unsigned and take lower 8 bits for a byte value
        var byteValue = (byte)(hash & 0xFF);
        
        // Convert to lowercase hex string
        return byteValue.ToString("x2");
    }

}