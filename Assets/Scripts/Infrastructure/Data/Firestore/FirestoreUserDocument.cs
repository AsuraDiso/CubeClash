using System.Collections.Generic;
using Firebase.Firestore;

namespace Infrastructure.Data.Firestore
{
    [FirestoreData]
    public struct FirestoreUserDocument
    {
        [FirestoreProperty]
        public string DisplayName { get; set; }

        [FirestoreProperty]
        public string UpdatedBy { get; set; }

        [FirestoreProperty]
        public int SelectedDeckIndex { get; set; }

        [FirestoreProperty]
        public List<FirestoreDeckSlotDocument> Decks { get; set; }
    }
}
