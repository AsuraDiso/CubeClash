using System;
using System.Collections.Generic;

namespace Game.Scripts.Core.Data
{
    public sealed class DeckPersistenceData
    {
        public int SelectedDeckIndex { get; set; }

        public IReadOnlyList<IReadOnlyList<DeckCardRecord>> Decks { get; set; } = Array.Empty<IReadOnlyList<DeckCardRecord>>();
    }
}
