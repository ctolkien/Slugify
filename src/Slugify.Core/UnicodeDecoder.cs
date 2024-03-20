using System.Linq;
using System.Text;

namespace Slugify.Core;

//Note: Lifted from: https://github.com/polischuk/SlugGenerator
internal class UnicodeDecoder
{
    /// <summary>
    /// Transliterate Unicode string to ASCII string.
    /// </summary>
    /// <param name="input">String you want to transliterate into ASCII</param>
    /// <param name="tempStringBuilderCapacity">
    ///     If you know the length of the result,
    ///     pass the value for StringBuilder capacity.
    ///     InputString.Length*2 is used by default.
    /// </param>
    /// <returns>
    ///     ASCII string. There are [?] (3 characters) in places of some unknown(?) unicode characters.
    ///     It is this way in Python code as well.
    /// </returns>
    public static string UniDecode(string input, int? tempStringBuilderCapacity = null)
    {
        if (string.IsNullOrEmpty(input))
        {
            return string.Empty;
        }

        //If we have only ASCII characters, return the input string
        if (input.All(x => x < 0x80))
        {
            return input;
        }

        var sb = new StringBuilder(tempStringBuilderCapacity ?? input.Length * 2);
        foreach (char c in input)
        {
            if (c < 0x80)
            {
                sb.Append(c);
            }
            else
            {
                int high = c >> 8;
                int low = c & 0xff;
                if (UnicodeDecoderCharacterMap.Characters.TryGetValue(high, out string[] transliterations))
                {
                    sb.Append(transliterations[low]);
                }
            }
        }

        return sb.ToString();
    }
}
