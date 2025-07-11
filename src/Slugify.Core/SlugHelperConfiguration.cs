using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Slugify;

/// <summary>
/// Used to configure the a <see cref="SlugHelper"/> instance
/// </summary>
public class SlugHelperConfiguration
{
    // TODO: Implement a source generator so this can be done at compile time :)
    private static readonly char[] _allowedCharsArray =
        [
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm',
            'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M',
            'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            '-', '.', '_'
        ];

    private readonly HashSet<char> _allowedChars = [.. _allowedCharsArray];

    /// <summary>
    /// Provides a way to replace strings with other strings before the slug is generated. By default, spaces are replaced with dashes.
    /// </summary>
    public Dictionary<string, string> StringReplacements { get; set; } = new Dictionary<string, string>
            {
                { " ", "-" }
            };

    /// <summary>
    /// Option to force the output to be lower case. Defaults to true.
    /// </summary>
    public bool ForceLowerCase { get; set; } = true;

    /// <summary>
    /// Provide a custom regex to match characters that should be removed from the slug.
    /// Note: Setting this property will stop the AllowedChars feature from being used and it will use this regex instead.
    /// A reasonable baseline would be `[^a-zA-Z0-9\-\._]` which allows letters, numbers, dashes, underscores, and periods
    /// </summary>
    public Regex? DeniedCharactersRegex { get; set; }

    /// <summary>
    /// This is a <c>HashSet</c> of characters that are allowed in the slug.
    /// By default , it contains all letters, numbers, dashes, underscores, and periods.
    /// You can add and remove to this collection to customize the allowed characters.
    /// </summary>
    public HashSet<char> AllowedCharacters =>
        DeniedCharactersRegex == null ?
        _allowedChars : throw new InvalidOperationException("After setting DeniedCharactersRegex the AllowedChars feature cannot be used.");

    /// <summary>
    /// Option to collapse multiple dashes into a single dash. Defaults to true.
    /// </summary>
    public bool CollapseDashes { get; set; } = true;

    /// <summary>
    /// Option to trim leading and trailing whitespace. Defaults to true.
    /// </summary>
    public bool TrimWhitespace { get; set; } = true;

    /// <summary>
    /// Represents the maximum length of the slug. If the slug is longer than this value, it will be truncated.
    /// </summary>
    public int? MaximumLength { get; set; }

    /// <summary>
    /// When enabled, slugs that exceed MaximumLength will be shortened with a hash postfix to ensure uniqueness.
    /// The hash postfix is a suffix derived from the full slug before truncation.
    /// Defaults to false for backward compatibility.
    /// </summary>
    public bool EnableHashedShortening { get; set; }

    /// <summary>
    /// Length of the hash postfix when EnableHashedShortening is true.
    /// Valid values are 2-6 characters. Defaults to 2 for backward compatibility.
    /// Higher values provide better collision resistance but use more characters.
    /// </summary>
    public int HashLength { get; set; } = 2;

    /// <summary>
    /// Enable non-ASCII languages support. Defaults to false
    /// </summary>
    public bool SupportNonAsciiLanguages { get; set; }

}

