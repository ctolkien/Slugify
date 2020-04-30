namespace Slugify.Core.Pipes {

    abstract class CharPipe {

        protected CharPipe Continuation;

        public T Continue<T>(T continuation) where T : CharPipe {
            Continuation = continuation;
            return continuation;
        }

        public abstract void Write(in Context context, char c);
        public abstract void Complete(in Context context);
    }
}
