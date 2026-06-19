using System.Collections.Generic;
using Cards;

namespace Core.Battle
{
    public interface IBattleLoadoutProvider
    {
        public IReadOnlyList<PlacedCard> SelectedDeck { get; }
    }
}
