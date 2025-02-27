namespace ABCD.Lib {
    public static class Guard {
        public static void ThrowIfNull(params object[] args) {
            for (int i=0;  i < args.Length; i++) {
                if (args[i] == null)
                    throw new ArgumentNullException($"args-{i}"); //can figure out actual arg from stack trace
            }
        }
    }
}
