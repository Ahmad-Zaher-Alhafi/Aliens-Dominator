namespace Context {
    public static class Ctx {
        public static IControllers Deps { get; private set; }

        public static void ContextChanged(Controllers controllers) {
            Deps = controllers;
        }
    }
}