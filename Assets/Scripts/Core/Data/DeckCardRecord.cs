namespace Core.Data
{
    public readonly struct DeckCardRecord
    {
        public int CatalogIndex { get; }
        public int OriginX { get; }
        public int OriginY { get; }

        public DeckCardRecord(int catalogIndex, int originX, int originY)
        {
            CatalogIndex = catalogIndex;
            OriginX = originX;
            OriginY = originY;
        }
    }
}
