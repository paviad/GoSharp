namespace GoSharpCore {
    /// <summary>
    /// Provides extension methods for the Go.Content enum.
    /// </summary>
    public static class ContentExtensions {
        /// <summary>
        /// Returns the opposite color.
        /// </summary>
        /// <param name="c">The color whose opposite is requested.
        /// Must be Content.Black or Content.White.</param>
        /// <returns>The opposite color.</returns>
        public static Content Opposite(this Content c) {
            return c == Content.Black ? Content.White : Content.Black;
        }
    }
}
