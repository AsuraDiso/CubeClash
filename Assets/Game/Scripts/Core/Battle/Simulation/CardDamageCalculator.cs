using System;
using Game.Scripts.Core.Data.Cards;

namespace Game.Scripts.Core.Battle.Simulation
{
    public static class CardDamageCalculator
    {
        public static int Resolve(ValueFormula formula, ReadOnlySpan<int> diceValues)
        {
            if (diceValues.Length == 0)
                return 0;

            return formula switch
            {
                ValueFormula.FirstDie => diceValues[0],
                ValueFormula.Sum => Sum(diceValues),
                ValueFormula.Average => Average(diceValues),
                ValueFormula.AverageTimes => Average(diceValues) * diceValues.Length,
                ValueFormula.Max => Max(diceValues),
                ValueFormula.Min => Min(diceValues),
                _ => 0,
            };
        }

        private static int Sum(ReadOnlySpan<int> values)
        {
            var total = 0;
            foreach (var value in values)
                total += value;

            return total;
        }

        private static int Average(ReadOnlySpan<int> values) =>
            (int)Math.Round((double)Sum(values) / values.Length);

        private static int Max(ReadOnlySpan<int> values)
        {
            var max = values[0];
            for (var i = 1; i < values.Length; i++)
            {
                if (values[i] > max)
                    max = values[i];
            }

            return max;
        }

        private static int Min(ReadOnlySpan<int> values)
        {
            var min = values[0];
            for (var i = 1; i < values.Length; i++)
            {
                if (values[i] < min)
                    min = values[i];
            }

            return min;
        }
    }
}
