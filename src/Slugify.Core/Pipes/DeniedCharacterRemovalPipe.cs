namespace Slugify.Core.Pipes {

    /// <summary>
    /// Removes denied characters from the slug. 
    /// The original api implementation had a regex in the config. 
    /// But I noticed that the regex was just the inverse of a list of allowed characters. 
    /// So I decided to hard-code the allowed characters and leave it at that. 
    /// It was a bit of a liberty with the original public API, but I think it's a sensible one.
    /// </summary>
    class DeniedCharacterRemovalPipe : CharPipe {
        public override void Write(in Context context, char c) {

            if (char.IsWhiteSpace(c)) {
                Continuation.Write(context, ' ');
            } else {
                var write = (c >= 'a' && c <= 'z')
                    || (c >= 'A' && c <= 'Z')
                    || (c >= '0' && c <= '9')
                    || (c == '.')
                    || (c == '_')
                    || (c == '-');

                if (write)
                    Continuation.Write(context, c);
            }
        }
        public override void Complete(in Context context) => Continuation.Complete(context);
    }
}
