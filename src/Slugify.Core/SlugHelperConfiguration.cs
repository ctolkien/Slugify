using System;
using System.Collections.Generic;

namespace Slugify
{
    /// <summary>
    /// Used to configure the a <see cref="SlugHelper"/> instance
    /// </summary>
    public class SlugHelperConfiguration
    {
        // TODO: Implement a source generator so this can be done at compile time :)
        private static readonly char[] s_allowedChars =
            ("abcdefghijklmnopqrstuvwxyz" +
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ" +
            "0123456789" +
            "-._").ToCharArray();

        private readonly HashSet<char> _allowedChars = new HashSet<char>(s_allowedChars);

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
        /// Option to collapse whitespace. Defaults to true.
        /// </summary>
        public bool CollapseWhiteSpace { get; set; } = true;

        /// <summary>
        /// Provide a custom regex to match characters that should be removed from the slug.
        /// Note: Setting this property will stop the AllowedChars feature from being used.
        /// </summary>
        public string DeniedCharactersRegex { get; set; }
        public HashSet<char> AllowedChars
        {
            get
            {
                return DeniedCharactersRegex == null ? _allowedChars : throw new InvalidOperationException("After setting DeniedCharactersRegex the AllowedChars feature cannot be used.");
            }
        }

        /// <summary>
        /// Option to collapse multiple dashes into a single dash. Defaults to true.
        /// </summary>
        public bool CollapseDashes { get; set; } = true;

        /// <summary>
        /// Option to trim leading and trailing whitespace. Defaults to true.
        /// </summary>
        public bool TrimWhitespace { get; set; } = true;
    }
}

