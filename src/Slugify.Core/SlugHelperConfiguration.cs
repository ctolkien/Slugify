﻿using System;
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
        ("abcdefghijklmnopqrstuvwxyz" +
        "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
        "0123456789" +
        "-._").ToCharArray();

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

}

