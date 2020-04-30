using System.Runtime.InteropServices;

namespace Slugify.Core.Pipes {

    /// <summary>
    /// Removes consecutive '-' characters.
    /// </summary>
    class CollapseDashesPipe : CharPipe {

        readonly int MemLocation;

        public CollapseDashesPipe(ref int memLocation) {
            MemLocation = memLocation;
            memLocation += sizeof(bool);
        }

        public override void Write(in Context context, char c) {
            var isDash = MemoryMarshal.Cast<byte, bool>(context.Memory.Slice(MemLocation));
            if (c == '-') {
                if (!isDash[0]) {
                    Continuation.Write(context, '-');
                    isDash[0] = true;
                }
            } else {
                isDash[0] = false;
                Continuation.Write(context, c);
            }
        }

        public override void Complete(in Context context) => Continuation.Complete(context);
    }
}
