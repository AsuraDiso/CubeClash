using System;
using System.Collections.Generic;
using System.Threading;
using Bootstrap.Common;
using Cards;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Core.Data
{
    public sealed class DeckService : IDeckService
    {
        private readonly DeckState _state;
        private readonly IDeckRepository _repository;
        private readonly CardCatalog _catalog;

        private bool _isLoaded;
        private bool _isSaving;

        public DeckService(IDeckRepository repository, CardCatalog catalog)
        {
            _repository = repository;
            _catalog = catalog;
            _state = new DeckState(DeckLayout.MaxDecks);
        }

        public int MaxDecks => _state.MaxDecks;
        public bool IsLoaded => _isLoaded;
        public int SelectedDeckIndex => _state.SelectedDeckIndex;
        public event Action<int> OnDeckChanged;

        public async UniTask LoadAsync(CancellationToken cancellationToken = default)
        {
            if (_isLoaded)
            {
                return;
            }

            var data = await _repository.LoadAsync(cancellationToken);
            ApplyPersistenceData(data);
            _isLoaded = true;
        }

        public IReadOnlyList<PlacedCard> GetDeck(int deckIndex) => _state.GetDeck(deckIndex);

        public bool TryPlaceCard(int deckIndex, CardDefinition card, Vector2Int origin, int catalogIndex)
        {
            if (!_state.TryPlaceCard(deckIndex, card, origin, catalogIndex))
            {
                return false;
            }

            NotifyChanged(deckIndex);
            return true;
        }

        public bool TryRemoveCard(int deckIndex, int catalogIndex)
        {
            if (!_state.TryRemoveCard(deckIndex, catalogIndex))
            {
                return false;
            }

            NotifyChanged(deckIndex);
            return true;
        }

        public void ClearDeck(int deckIndex)
        {
            var hadCards = _state.GetDeck(deckIndex).Count > 0;
            _state.ClearDeck(deckIndex);

            if (hadCards)
            {
                NotifyChanged(deckIndex);
            }
        }

        public void SetSelectedDeckIndex(int deckIndex)
        {
            if (deckIndex < 0 || deckIndex >= MaxDecks || deckIndex == _state.SelectedDeckIndex)
            {
                return;
            }

            _state.SelectedDeckIndex = deckIndex;
            ScheduleSave();
        }

        private void ApplyPersistenceData(DeckPersistenceData data)
        {
            _state.SelectedDeckIndex = Mathf.Clamp(data.SelectedDeckIndex, 0, MaxDecks - 1);

            for (var deckIndex = 0; deckIndex < MaxDecks; deckIndex++)
            {
                var records = data.Decks != null && deckIndex < data.Decks.Count
                    ? data.Decks[deckIndex]
                    : null;

                _state.SetDeck(deckIndex, DeckRecordConverter.ToPlacedCards(records, _catalog));
            }
        }

        private void NotifyChanged(int deckIndex)
        {
            OnDeckChanged?.Invoke(deckIndex);
            ScheduleSave();
        }

        private void ScheduleSave()
        {
            if (!_isLoaded || _isSaving)
            {
                return;
            }

            FireAndForget.Run(SaveAsync);
        }

        private async UniTask SaveAsync()
        {
            if (_isSaving)
            {
                return;
            }

            _isSaving = true;

            try
            {
                await _repository.SaveAsync(DeckRecordConverter.FromState(_state));
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                _isSaving = false;
            }
        }
    }
}
