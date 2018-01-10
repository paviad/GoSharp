using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using JetBrains.Annotations;

namespace Go {
    /// <summary>
    /// Represents an SGF property, see the SGF specification at
    /// <a href="http://www.red-bean.com/sgf">http://www.red-bean.com/sgf</a>
    /// </summary>
    public class SGFProperty {
        /// <summary>
        /// Contains the property name.
        /// </summary>
        public string Name;

        /// <summary>
        /// Contains a list of SGF property-value objects.
        /// </summary>
        public List<SGFPropValue> Values = new List<SGFPropValue>();

        /// <summary>
        /// Returns true if this property is a move property.
        /// </summary>
        [PublicAPI]
        public bool IsMove => Name == "W" || Name == "B";

        /// <summary>
        /// Returns true if this property is a setup property.
        /// </summary>
        [PublicAPI]
        public bool IsSetup => Name == "AE" || Name == "AB" || Name == "AW" || Name == "PL";

        private readonly HashSet<string> _moveProperties = new HashSet<string>
        {
            "W","B","AB","AW","AE"
        };
        /// <summary>
        /// Returns true if this property is a file format property.
        /// </summary>
        public bool IsRoot => !_moveProperties.Contains(Name);

        /// <summary>
        /// Returns the property priority when writing an SGF file.
        /// </summary>
        public int Priority {
            get {
                if (IsRoot) return 0;
                else if (IsSetup) return 1;
                else if (IsMove) return 2;
                else return 3;
            }
        }

        internal void Read(TextReader sr) {
            Name = "";
            sr.EatWs();
            while (char.IsUpper((char)sr.Peek())) {
                var c = (char)sr.Read();
                Name += c;
            }
            sr.EatWs();
            while (sr.Peek() == '[') {
                ReadValue(sr);
                sr.EatWs();
            }
        }

        private void ReadValue(TextReader sr) {
            var c = (char)sr.Read();
            if (c != '[')
                throw new InvalidDataException("Property value doesn't begin with a '['.");

            var verbatim = false;
            var sb = new StringBuilder();

            for (; ; ) {
                c = (char)sr.Read();

                /* Spec 3.2. Text/Formatting
                 * Formatting:
                 * Soft line break: linebreaks preceded by a "\" (soft linebreaks are converted to "", i.e. they are removed)
                 * Hard line breaks: any other linebreaks encountered
                 *
                 * Attention: a single linebreak is represented differently on different systems, e.g. "LFCR" for DOS, "LF" on Unix. An application should be able to deal with following linebreaks: LF, CR, LFCR, CRLF.
                 * [...]
                 * Escaping: "\" is the escape character. Any char following "\" is inserted verbatim (exception: whitespaces still have to be converted to space!). Following chars have to be escaped, when used in Text: "]", "\" and ":" (only if used in compose data type).
                 */
                if (verbatim) {
                    if (c == '\r' || c == '\n') {
                        var next = sr.Peek();
                        if (next != c && (next == '\r' || next == '\n')) {
                            sr.Read();
                        }
                    } else {
                        if (char.IsWhiteSpace(c)) {
                            c = ' ';
                        }
                        sb.Append(c);
                    }
                    verbatim = false;
                    continue;
                }

                if (c == '\\') {
                    verbatim = true;
                    continue;
                }

                if (c == ']') {
                    break;
                }

                sb.Append(c);
            }

            Values.Add(new SGFPropValue(sb.ToString()));
        }

        /// <inheritdoc />
        public override string ToString() {
            var vs = Values.Select(v => v.ToString()).ToArray();
            return Name + ":" + string.Join(", ", vs);
        }
    }
}
