using System.IO;

namespace GoSharpCore {
    /// <summary>
    /// Provides extension methods for TextReader. Contains these methods:
    /// <list type="string">
    /// <item>EatWS</item>
    /// </list>
    /// </summary>
    public static class TextReaderExtensions {
        /// <summary>
        /// Reads and ignores white space characters, until a non-whitespace
        /// is encountered.
        /// </summary>
        /// <param name="tr">The TextReader object.</param>
        public static void EatWS(this TextReader tr) {
            while (char.IsWhiteSpace((char)tr.Peek())) {
                tr.Read();
            }
        }
    }
}
