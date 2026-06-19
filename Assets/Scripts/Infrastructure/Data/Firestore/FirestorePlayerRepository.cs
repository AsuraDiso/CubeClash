using System;
using System.Collections.Generic;
using System.Threading;
using Core.Data;
using Core.Firebase;
using Cysharp.Threading.Tasks;
using Firebase.Firestore;
using UnityEngine;

namespace Infrastructure.Data.Firestore
{
    public sealed class FirestorePlayerRepository : IPlayerRepository
    {
        private const string UsersCollection = "users";
        private const string DefaultDisplayName = PlayerProfile.DefaultDisplayName;
        private const string SystemUpdatedBy = "system";

        private readonly IFirebaseAppService _firebaseAppService;
        private readonly IUserIdProvider _userIdProvider;

        private PlayerProfile _profile;

        public FirestorePlayerRepository(IFirebaseAppService firebaseAppService, IUserIdProvider userIdProvider)
        {
            _firebaseAppService = firebaseAppService;
            _userIdProvider = userIdProvider;
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
            {
                return;
            }

            await _firebaseAppService.EnsureInitializedAsync(cancellationToken);

            var userId = _userIdProvider.UserId;
            var document = await LoadOrCreateUserDocumentAsync(userId, cancellationToken);
            var displayName = string.IsNullOrWhiteSpace(document.DisplayName)
                ? DefaultDisplayName
                : document.DisplayName;

            _profile = PlayerProfile.CreateBattleDefault(userId, displayName);
            IsLoaded = true;
            Debug.Log($"Player loaded: {_profile.DisplayName}");
        }

        public async UniTask UpdateDisplayNameAsync(string displayName, CancellationToken cancellationToken = default)
        {
            EnsureLoaded();

            var trimmed = displayName?.Trim();
            if (string.IsNullOrEmpty(trimmed))
            {
                trimmed = DefaultDisplayName;
            }

            if (trimmed == _profile.DisplayName)
            {
                return;
            }

            await _firebaseAppService.EnsureInitializedAsync(cancellationToken);

            var userRef = FirebaseFirestore.DefaultInstance
                .Collection(UsersCollection)
                .Document(_userIdProvider.UserId);

            await userRef
                .UpdateAsync(new Dictionary<string, object>
                {
                    { nameof(FirestoreUserDocument.DisplayName), trimmed },
                    { nameof(FirestoreUserDocument.UpdatedBy), "player" }
                })
                .AsUniTask()
                .AttachExternalCancellation(cancellationToken);

            _profile = PlayerProfile.CreateBattleDefault(_profile.PlayerId, trimmed);
            ProfileUpdated?.Invoke();
        }

        private static async UniTask<FirestoreUserDocument> LoadOrCreateUserDocumentAsync(
            string userId,
            CancellationToken cancellationToken)
        {
            var userRef = FirebaseFirestore.DefaultInstance
                .Collection(UsersCollection)
                .Document(userId);

            var snapshot = await userRef
                .GetSnapshotAsync()
                .AsUniTask()
                .AttachExternalCancellation(cancellationToken);

            if (snapshot.Exists)
            {
                return snapshot.ConvertTo<FirestoreUserDocument>();
            }

            var newUser = new FirestoreUserDocument
            {
                DisplayName = DefaultDisplayName,
                UpdatedBy = SystemUpdatedBy
            };

            await userRef
                .SetAsync(newUser)
                .AsUniTask()
                .AttachExternalCancellation(cancellationToken);

            return newUser;
        }

        private void EnsureLoaded()
        {
            if (!IsLoaded)
            {
                throw new InvalidOperationException("Player data has not been loaded yet.");
            }
        }
    }
}
