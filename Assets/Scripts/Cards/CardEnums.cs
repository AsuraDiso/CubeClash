namespace Cards
{
    public enum CardFootprint
    {
        OneByOne = 1,
        OneByTwo = 2,
    }
    public enum CardLayout
    {
        Horizontal,
        Vertical
    }
    public enum CardRarity
    {
        Common,
        Rare,
        Epic,
        Legendary
    }
    public enum ValueFormula
    {
        FirstDie,
        Average,
        AverageTimes,
        Sum,
        Max,
        Min,
    }
}
