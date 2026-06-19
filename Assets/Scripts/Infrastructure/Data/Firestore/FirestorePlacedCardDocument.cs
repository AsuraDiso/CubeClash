using Firebase.Firestore;

namespace Infrastructure.Data.Firestore
{
    [FirestoreData]
    public struct FirestorePlacedCardDocument
    {
        [FirestoreProperty]
        public int CatalogIndex { get; set; }

        [FirestoreProperty]
        public int OriginX { get; set; }

        [FirestoreProperty]
        public int OriginY { get; set; }
    }
}
