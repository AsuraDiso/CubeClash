using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Bootstrap.Common
{
    public static class FireAndForget
    {
        public static void Run(Func<UniTask> task) => task().Forget(HandleException);

        public static void Run(Func<CancellationToken, UniTask> task, CancellationToken cancellationToken) =>
            task(cancellationToken).Forget(HandleException);

        private static void HandleException(Exception exception)
        {
            if (exception is OperationCanceledException)
            {
                return;
            }

            Debug.LogException(exception);
        }
    }
}
