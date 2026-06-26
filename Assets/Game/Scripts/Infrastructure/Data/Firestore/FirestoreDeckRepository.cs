using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Firebase.Firestore;
using Game.Scripts.Core.Data;
using Game.Scripts.Core.Firebase;

namespace Game.Scripts.Infrastructure.Data.Firestore
{
    public sealed class FirestoreDeckRepository : IDeckRepository
    {
        private readonly IFirebaseAppService _firebaseAppService;
        private readonly FirestoreUserDocumentStore _documentStore;

        public FirestoreDeckRepository(IFirebaseAppService firebaseAppService, FirestoreUserDocumentStore documentStore)
        {
            _firebaseAppService = firebaseAppService;
            _documentStore = documentStore;
        }

        public async UniTask<DeckPersistenceData> LoadAsync(CancellationToken cancellationToken = default)
        {
            await _documentStore.EnsureLoadedAsync(cancellationToken);
            if (!_documentStore.Exists)
                return new DeckPersistenceData();

            return FirestoreDeckDocumentMapper.ToPersistenceData(_documentStore.Document);
        }

        public async UniTask SaveAsync(DeckPersistenceData data, CancellationToken cancellationToken = default)
        {
            await _firebaseAppService.EnsureInitializedAsync(cancellationToken);

            var deckUpdate = new Dictionary<string, object>
            {
                { nameof(FirestoreUserDocument.SelectedDeckIndex), data.SelectedDeckIndex },
                { nameof(FirestoreUserDocument.Decks), FirestoreDeckDocumentMapper.ToFirestoreDecks(data) }
            };

            try
            {
                await _documentStore.UserRef.UpdateAsync(deckUpdate).AsUniTask().AttachExternalCancellation(cancellationToken);
            }
            catch (Exception exception) when (FirestoreExceptionHelper.IsNotFound(exception))
            {
                await _documentStore.UserRef.SetAsync(deckUpdate, SetOptions.MergeAll).AsUniTask().AttachExternalCancellation(cancellationToken);
            }
        }
    }
}
