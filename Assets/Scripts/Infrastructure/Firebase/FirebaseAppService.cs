using System;
using System.Threading;
using Core.Firebase;
using Cysharp.Threading.Tasks;
using Firebase;
using UnityEngine;

namespace Infrastructure.Firebase
{
    public sealed class FirebaseAppService : IFirebaseAppService
    {
        private UniTask? _initializationTask;

        public bool IsInitialized { get; private set; }

        public UniTask EnsureInitializedAsync(CancellationToken cancellationToken = default)
        {
            if (IsInitialized)
                return UniTask.CompletedTask;

            return (_initializationTask ??= InitializeInternalAsync(cancellationToken))
                .AttachExternalCancellation(cancellationToken);
        }

        private async UniTask InitializeInternalAsync(CancellationToken cancellationToken)
        {
            var status = await FirebaseApp
                .CheckAndFixDependenciesAsync()
                .AsUniTask()
                .AttachExternalCancellation(cancellationToken);

            if (status != DependencyStatus.Available)
                throw new InvalidOperationException($"Firebase dependencies are unavailable: {status}");

            IsInitialized = true;
            Debug.Log("Firebase ready.");
        }
    }
}
