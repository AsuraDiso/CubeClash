using System;
using System.Collections.Generic;
using System.Threading;
using Core.Data;
using Core.Firebase;
using Cysharp.Threading.Tasks;
using Firebase.Firestore;
using Infrastructure.Data.Firestore;
using UnityEngine;

namespace Infrastructure.Data.Firestore
{
    public sealed class FirestoreDeckRepository : IDeckRepository
    {
        private const string UsersCollection = "users";

        private readonly IFirebaseAppService _firebaseAppService;
        private readonly IUserIdProvider _userIdProvider;

        public FirestoreDeckRepository(IFirebaseAppService firebaseAppService, IUserIdProvider userIdProvider)
        {
            _firebaseAppService = firebaseAppService;
            _userIdProvider = userIdProvider;
        }

        public async UniTask<DeckPersistenceData> LoadAsync(CancellationToken cancellationToken = default)
        {
            await _firebaseAppService.EnsureInitializedAsync(cancellationToken);

            var snapshot = await UserRef
                .GetSnapshotAsync()
                .AsUniTask()
                .AttachExternalCancellation(cancellationToken);

            if (!snapshot.Exists)
            {
                return new DeckPersistenceData();
            }

            var document = snapshot.ConvertTo<FirestoreUserDocument>();
            return ToPersistenceData(document);
        }

        public async UniTask SaveAsync(DeckPersistenceData data, CancellationToken cancellationToken = default)
        {
            await _firebaseAppService.EnsureInitializedAsync(cancellationToken);

            var deckUpdate = new Dictionary<string, object>
            {
                { nameof(FirestoreUserDocument.SelectedDeckIndex), data.SelectedDeckIndex },
                { nameof(FirestoreUserDocument.Decks), ToFirestoreDecks(data) }
            };

            try
            {
                await UserRef.UpdateAsync(deckUpdate).AsUniTask().AttachExternalCancellation(cancellationToken);
            }
            catch (Exception exception) when (exception.Message.Contains("NOT_FOUND", StringComparison.OrdinalIgnoreCase))
            {
                await UserRef.SetAsync(deckUpdate, SetOptions.MergeAll).AsUniTask().AttachExternalCancellation(cancellationToken);
            }
        }

        private DocumentReference UserRef => FirebaseFirestore.DefaultInstance
            .Collection(UsersCollection)
            .Document(_userIdProvider.UserId);

        private static DeckPersistenceData ToPersistenceData(FirestoreUserDocument document)
        {
            var decks = new List<IReadOnlyList<DeckCardRecord>>();
            if (document.Decks != null)
            {
                foreach (var deck in document.Decks)
                {
                    decks.Add(ToRecords(deck.Cards));
                }
            }

            return new DeckPersistenceData
            {
                SelectedDeckIndex = document.SelectedDeckIndex,
                Decks = decks
            };
        }

        private static List<FirestoreDeckSlotDocument> ToFirestoreDecks(DeckPersistenceData data)
        {
            var decks = new List<FirestoreDeckSlotDocument>();
            if (data.Decks == null)
            {
                return decks;
            }

            foreach (var deck in data.Decks)
            {
                var cards = new List<FirestorePlacedCardDocument>();
                if (deck != null)
                {
                    foreach (var record in deck)
                    {
                        cards.Add(new FirestorePlacedCardDocument
                        {
                            CatalogIndex = record.CatalogIndex,
                            OriginX = record.OriginX,
                            OriginY = record.OriginY
                        });
                    }
                }

                decks.Add(new FirestoreDeckSlotDocument { Cards = cards });
            }

            return decks;
        }

        private static List<DeckCardRecord> ToRecords(List<FirestorePlacedCardDocument> cards)
        {
            var records = new List<DeckCardRecord>();
            if (cards == null)
            {
                return records;
            }

            foreach (var card in cards)
            {
                records.Add(new DeckCardRecord(card.CatalogIndex, card.OriginX, card.OriginY));
            }

            return records;
        }
    }
}
