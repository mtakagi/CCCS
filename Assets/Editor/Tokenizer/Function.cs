using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CCCS
{
    public class Function
    {

        public Function Next { get; internal set; }
        public Node Node { get; internal set; }
        public string Name { get; internal set; }
        public LocalVariable Locals { get; internal set; }
        public int StackSize { get; internal set; }
    }

}
