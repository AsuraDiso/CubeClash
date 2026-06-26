using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Firebase.Firestore;
using Game.Scripts.Core.Data;
using Game.Scripts.Core.Firebase;

namespace Game.Scripts.Infrastructure.Data.Firestore
{
    public sealed class FirestorePlayerRepository : IPlayerRepository
    {
        private const string SystemUpdatedBy = "system";

        private readonly IFirebaseAppService _firebaseAppService;
        private readonly FirestoreUserDocumentStore _documentStore;
        private PlayerProfile _profile;

        public FirestorePlayerRepository(IFirebaseAppService firebaseAppService, FirestoreUserDocumentStore documentStore)
        {
            _firebaseAppService = firebaseAppService;
            _documentStore = documentStore;
        }

        public bool IsLoaded { get; private set; }

        public PlayerProfile Profile
        {
            get
            {
                EnsureLoaded();
                return _profile;
            }
        }

        public event Action ProfileUpdated;

        public async UniTask LoadAsync(CancellationToken cancellationToken = default)
        {
            if (IsLoaded)
                return;

            var document = await _documentStore.GetOrCreateAsync(
                new FirestoreUserDocument
                {
                    DisplayName = PlayerProfile.DefaultDisplayName,
                    UpdatedBy = SystemUpdatedBy
                },
                cancellationToken);

            var displayName = PlayerProfile.NormalizeDisplayName(document.DisplayName);

            _profile = PlayerProfile.CreateBattleDefault(_documentStore.UserId, displayName);
            IsLoaded = true;
        }

        public async UniTask UpdateDisplayNameAsync(string displayName, CancellationToken cancellationToken = default)
        {
            EnsureLoaded();

            var trimmed = PlayerProfile.NormalizeDisplayName(displayName);
            if (trimmed == _profile.DisplayName)
                return;

            await _firebaseAppService.EnsureInitializedAsync(cancellationToken);
            await UpdateDisplayNameInFirestoreAsync(trimmed, cancellationToken);

            _profile = PlayerProfile.CreateBattleDefault(_profile.PlayerId, trimmed);
            ProfileUpdated?.Invoke();
        }

        private UniTask UpdateDisplayNameInFirestoreAsync(string trimmed, CancellationToken cancellationToken) =>
            _documentStore.UserRef
                .UpdateAsync(new Dictionary<string, object>
                {
                    { nameof(FirestoreUserDocument.DisplayName), trimmed },
                    { nameof(FirestoreUserDocument.UpdatedBy), "player" }
                })
                .AsUniTask()
                .AttachExternalCancellation(cancellationToken);

        private void EnsureLoaded()
        {
            if (!IsLoaded)
                throw new InvalidOperationException("Player data has not been loaded yet.");
        }
    }
}
