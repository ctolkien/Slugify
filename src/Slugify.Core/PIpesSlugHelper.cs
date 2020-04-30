using System;
using System.Collections.Generic;
using System.Text;

namespace Slugify.Core {
    class PipesSlugHelper : ISlugHelper {

        readonly SlugHelper.Config Config;

        readonly CharPipe StartPipe;
        readonly ResultCharPipe ResultCharPipe;

        public PipesSlugHelper(SlugHelper.Config config) {
            Config = config;
            var currentPipe = StartPipe = new NoOpCharPipe();

            if (Config.TrimWhitespace)
                currentPipe = currentPipe.Continue(new TrimCharPipe());

            ResultCharPipe = currentPipe.Continue(new ResultCharPipe());
        }

        public string GenerateSlug(string inputString) {
            foreach (var c in inputString)
                StartPipe.Write(c);
            StartPipe.Complete();
            return ResultCharPipe.GetResult();
        }
    }
}
