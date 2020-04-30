using System.Globalization;

namespace Slugify.Core.Pipes {

    /// <summary>
    /// After a string has been normalized to FormD, the diacritic characters are formed with a "non spacing mark" that follows
    /// the base character and defines the diacritic symbol. Removing these characters converts the diacritic character back to 
    /// a standard character.
    /// This pipe can be used correctly only if the string has already been normalized to Form D.
    /// </summary>
    class DiacriticRemoverPipe : CharPipe {
        public override void Write(in Context context, char c) {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                Continuation.Write(context, c);
        }
        public override void Complete(in Context context) => Continuation.Complete(context);
    }
}
