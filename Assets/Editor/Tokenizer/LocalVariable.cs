using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

namespace CCCS
{
    public class LocalVariable
    {
        public string Name { get; private set; }
        public int Offset { get; internal set; }

        public LocalVariable(string name, int offset)
        {
            Name = name;
            Offset = offset;
        }

    }
}
