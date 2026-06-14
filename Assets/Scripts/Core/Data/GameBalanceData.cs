namespace Core.Data
{
    public sealed class GameBalanceData
    {
        public int MaxHp { get; }
        public int AttackDamage { get; }

        public GameBalanceData(int maxHp, int attackDamage)
        {
            MaxHp = maxHp;
            AttackDamage = attackDamage;
        }
    }
}
