using System;
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
        const string UsersCollection = "users";
        const string DefaultDisplayName = PlayerProfile.DefaultDisplayName;
        const string SystemUpdatedBy = "system";

        readonly IFirebaseAppService _firebaseAppService;
        readonly IUserIdProvider _userIdProvider;

        PlayerProfile _profile;

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

        static async UniTask<FirestoreUserDocument> LoadOrCreateUserDocumentAsync(
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

        void EnsureLoaded()
        {
            if (!IsLoaded)
            {
                throw new InvalidOperationException("Player data has not been loaded yet.");
            }
        }
    }
}
