namespace Slugify.Core.Pipes {


    /// <summary>
    /// Converts the slug to lower case. 
    /// It does not convert ToLowerInvariant. I wasn't sure if it should use ToLower() to ToLowerInvariant()
    /// </summary>
    class ToLowerCharPipe : CharPipe {
        public override void Write(in Context context, char c) => Continuation.Write(context, char.ToLower(c));
        public override void Complete(in Context context) => Continuation.Complete(context);
    }
}
