using System;

namespace CCCS
{
    public class VariableList
    {
        public VariableList Next { get; internal set; }
        public LocalVariable Var { get; internal set; }

        public LocalVariable Find(Predicate<LocalVariable> predicate)
        {
            for (var val = this; val != null; val = this.Next)
            {
                if (predicate(val.Var))
                {
                    return val.Var;
                }
            }

            return null;
        }
    }
}
