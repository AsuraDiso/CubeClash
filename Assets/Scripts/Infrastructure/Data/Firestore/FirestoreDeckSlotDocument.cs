using System.Collections.Generic;
using Firebase.Firestore;

namespace Infrastructure.Data.Firestore
{
    [FirestoreData]
    public struct FirestoreDeckSlotDocument
    {
        [FirestoreProperty]
        public List<FirestorePlacedCardDocument> Cards { get; set; }
    }
}
