using System.Collections.Generic;
using System.Text;

namespace Slugify.Core.Pipes {

    /// <summary>
    /// This is the most important pipe! It must always be the last pipe added.
    /// </summary>
    class ResultCharPipe : CharPipe {

        public override void Write(in Context context, char c) {
            /// Write the character directly to the result char span in the context, 
            /// updating the number of characters that have been written.
            context.Result[context.ResultCount[0]++] = c;
        }

        public override void Complete(in Context context) {
            /// There are no more pipes to update!
        }
    }
}
