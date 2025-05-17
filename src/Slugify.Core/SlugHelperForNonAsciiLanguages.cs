using Slugify.Core;
using System;

namespace Slugify;

/// <summary>
/// This is a <see cref="SlugHelper"/> that is designed to work with non-ASCII languages.
/// </summary>
[Obsolete("Use SlugHelper with SupportNonAsciiLanguages option instead")]
public class SlugHelperForNonAsciiLanguages : SlugHelper
{
    public SlugHelperForNonAsciiLanguages()
    {
    }

    public SlugHelperForNonAsciiLanguages(SlugHelperConfiguration config) : base(config)
    {
    }

    public override string GenerateSlug(string inputString)
    {
        inputString = UnicodeDecoder.UniDecode(inputString);

        return base.GenerateSlug(inputString);
    }
}

