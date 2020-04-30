using System.Runtime.InteropServices;

namespace Slugify.Core.Pipes {

    /// <summary>
    /// Replaces all consecutive whitespace characters with a single '-' character.
    /// Any single whitespace character is converted to a '-' character.
    /// </summary>
    class CollapseWhiteSpacePipe : CharPipe {
        readonly int MemLocation;
        public CollapseWhiteSpacePipe(ref int memLocation) {
            MemLocation = memLocation;
            memLocation += sizeof(bool);
        }
        public override void Write(in Context context, char c) {
            var isWhitespace = MemoryMarshal.Cast<byte, bool>(context.Memory.Slice(MemLocation));
            if (char.IsWhiteSpace(c)) {
                isWhitespace[0] = true;
            } else {
                if (isWhitespace[0]) {
                    Continuation.Write(context, '-');
                    isWhitespace[0] = false;
                }
                Continuation.Write(context, c);
            }
        }
        public override void Complete(in Context context) {
            Continuation.Complete(context);
        }
    }
}
