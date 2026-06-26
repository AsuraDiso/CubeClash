using System;
using Firebase.Firestore;

namespace Game.Scripts.Infrastructure.Data.Firestore
{
    internal static class FirestoreExceptionHelper
    {
        public static bool IsNotFound(Exception exception)
        {
            for (var current = exception; current != null; current = current.InnerException)
            {
                if (current is FirestoreException firestoreException
                    && firestoreException.ErrorCode == FirestoreError.NotFound)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
