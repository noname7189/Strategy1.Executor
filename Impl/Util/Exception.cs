using System.Reflection;

namespace Strategy1.Executor.Impl.Util
{
    public class BadDesignException : Exception
    {
        private string CallerMessage { get; set; } = string.Empty;

        public BadDesignException()
            : base() { }

        public BadDesignException(string s)
            : base(s)
        {
            MethodBase? caller = new System.Diagnostics.StackTrace().GetFrame(1)?.GetMethod();
            var className = caller?.DeclaringType?.Name;
            var methodName = caller?.Name;

            if (className != null || methodName != null)
            {
                CallerMessage = $"[{className}][{methodName}]\n{s}";
            }
        }

        public BadDesignException(string message, Exception inner)
            : base(message, inner) { }

        public override string Message => $"{base.Message}\n{CallerMessage}";
    }
}
