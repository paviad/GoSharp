using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Go
{
    /// <summary>
    /// Represents an SGF property, see the SGF specification at
    /// <a href="http://www.red-bean.com/sgf">http://www.red-bean.com/sgf</a>
    /// </summary>
    public class SGFProperty
    {
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
        public bool IsMove { get { return Name == "W" || Name == "B"; } }

        /// <summary>
        /// Returns true if this property is a setup property.
        /// </summary>
        public bool IsSetup { get { return Name == "AE" || Name == "AB" || Name == "AW" || Name == "PL"; } }

        /// <summary>
        /// Returns true if this property is a file format property.
        /// </summary>
        public bool IsFileFormat { get { return Name == "FF"; } }

        /// <summary>
        /// Returns the property priority when writing an SGF file.
        /// </summary>
        public int Priority
        {
            get
            {
                if (IsFileFormat) return 0;
                if (IsSetup) return 1;
                if (IsMove) return 2;
                return 3;
            }
        }

        internal void Read(TextReader sr)
        {
            char c;
            Name = "";
            sr.EatWS();
            while (char.IsUpper((char)sr.Peek()))
            {
                c = (char)sr.Read();
                Name += c;
            }
            sr.EatWS();
            while (sr.Peek() == '[')
            {
                ReadValue(sr);
                sr.EatWS();
            }
        }

        private void ReadValue(TextReader sr)
        {
            char c = (char)sr.Read();
            if (c != '[')
                throw new InvalidDataException("Property value doesn't begin with a '['.");
            string val = "";
            while ((c = (char)sr.Read()) != ']')
            {
                val += c;
            }
            Values.Add(new SGFPropValue(val.Trim()));
        }
    }
}
