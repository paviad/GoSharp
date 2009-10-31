using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace Go
{
    class SGFProperty
    {
        public string Name;
        public List<SGFPropValue> Values = new List<SGFPropValue>();

        public bool IsMove { get { return Name == "W" || Name == "B"; } }
        public bool IsSetup { get { return Name == "AE" || Name == "AB" || Name == "AW" || Name == "PL"; } }
        public bool IsFileFormat { get { return Name == "FF"; } }

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
