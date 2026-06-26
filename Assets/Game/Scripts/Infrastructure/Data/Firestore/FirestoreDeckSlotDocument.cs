using System.Collections.Generic;
using Firebase.Firestore;

namespace Game.Scripts.Infrastructure.Data.Firestore
{
    [FirestoreData]
    public struct FirestoreDeckSlotDocument
    {
        [FirestoreProperty]
        public List<FirestorePlacedCardDocument> Cards { get; set; }
    }
}
