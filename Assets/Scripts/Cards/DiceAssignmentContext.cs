using System.Collections.Generic;

namespace Cards
{
    public sealed class DiceAssignmentContext
    {
        public IReadOnlyList<int> Values { get; }

        public DiceAssignmentContext( IReadOnlyList<int> values)
        {
            Values = values;
        }
    }
}
