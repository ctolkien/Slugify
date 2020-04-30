using Slugify.Core.Pipes;
using System;
using System.Collections.Generic;
using System.Text;

namespace Slugify.Core {
    public class PipesSlugHelper : ISlugHelper {

        readonly SlugHelper.Config Config;
        readonly CharPipe StartPipe;
        readonly ResultCharPipe ResultPipe;
        readonly int MemorySize;

        public PipesSlugHelper() : this(new SlugHelper.Config()) { }



        public PipesSlugHelper(SlugHelper.Config config) {
            Config = config ?? throw new ArgumentNullException(nameof(config));
            Build(out StartPipe, out ResultPipe, out MemorySize);
        }

        public string GenerateSlug(string inputString) {

            /// This is the first step of two steps required for removing diacritics.
            /// The second step is implemented in the first char pipe.
            if (!inputString.IsNormalized(NormalizationForm.FormD))
                inputString = inputString.Normalize(NormalizationForm.FormD);

            Span<byte> memory = MemorySize <= 1024 ? stackalloc byte[MemorySize] : new byte[MemorySize];
            var context = new Context(memory);
            foreach (var c in inputString)
                StartPipe.Write(context, c);
            StartPipe.Complete(context);
            return context.Result.ToString();
        }

        void Build(out CharPipe startPipe, out ResultCharPipe resultPipe, out int memorySize) {
            var memLocation = 0;
            var currentPipe = startPipe = new DiacriticRemoverPipe();

            if (Config.TrimWhitespace)
                currentPipe = currentPipe.Continue(new TrimCharPipe(ref memLocation));

            currentPipe = currentPipe.Continue(new DeniedCharacterRemovalPipe());

            foreach (var kvp in Config.StringReplacements)
                currentPipe = currentPipe.Continue(new ReplaceCharPipe(kvp.Key, kvp.Value, ref memLocation));

            if (Config.CollapseWhiteSpace)
                currentPipe = currentPipe.Continue(new CollapseWhiteSpacePipe(ref memLocation));

            if (Config.CollapseDashes)
                currentPipe = currentPipe.Continue(new CollapseDashesPipe(ref memLocation));

            if (Config.ForceLowerCase)
                currentPipe = currentPipe.Continue(new ToLowerCharPipe());

            resultPipe = currentPipe.Continue(new ResultCharPipe());
            memorySize = memLocation;
        }
    }
}
