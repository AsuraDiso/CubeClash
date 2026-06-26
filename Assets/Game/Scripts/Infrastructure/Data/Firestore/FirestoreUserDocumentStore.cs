using System.Threading;
using Cysharp.Threading.Tasks;
using Firebase.Firestore;
using Game.Scripts.Core.Data;
using Game.Scripts.Core.Firebase;

namespace Game.Scripts.Infrastructure.Data.Firestore
{
    public sealed class FirestoreUserDocumentStore
    {
        private const string UsersCollection = "users";

        private readonly IFirebaseAppService _firebaseAppService;
        private readonly IUserIdProvider _userIdProvider;

        private FirestoreUserDocument _document;
        private bool _isLoaded;

        public bool Exists { get; private set; }

        public FirestoreUserDocument Document => _document;

        public string UserId => _userIdProvider.UserId;

        public DocumentReference UserRef => FirebaseFirestore.DefaultInstance
            .Collection(UsersCollection)
            .Document(_userIdProvider.UserId);

        public FirestoreUserDocumentStore(IFirebaseAppService firebaseAppService, IUserIdProvider userIdProvider)
        {
            _firebaseAppService = firebaseAppService;
            _userIdProvider = userIdProvider;
        }

        public async UniTask EnsureLoadedAsync(CancellationToken cancellationToken = default)
        {
            if (_isLoaded)
                return;

            await _firebaseAppService.EnsureInitializedAsync(cancellationToken);

            var snapshot = await UserRef
                .GetSnapshotAsync()
                .AsUniTask()
                .AttachExternalCancellation(cancellationToken);

            Exists = snapshot.Exists;
            if (snapshot.Exists)
                _document = snapshot.ConvertTo<FirestoreUserDocument>();

            _isLoaded = true;
        }

        public async UniTask<FirestoreUserDocument> GetOrCreateAsync(
            FirestoreUserDocument defaultDocument,
            CancellationToken cancellationToken = default)
        {
            await EnsureLoadedAsync(cancellationToken);

            if (Exists)
                return _document;

            await UserRef
                .SetAsync(defaultDocument)
                .AsUniTask()
                .AttachExternalCancellation(cancellationToken);

            _document = defaultDocument;
            Exists = true;
            return _document;
        }
    }
}
