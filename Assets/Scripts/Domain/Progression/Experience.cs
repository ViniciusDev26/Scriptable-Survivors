using System;

namespace ScriptableSurvivors.Domain.Progression
{
    /// <summary>
    /// XP e nível da run. Guarda os níveis pendentes em vez de disparar um
    /// evento imediato: matar cinco inimigos com um tiro de canhão pode subir
    /// dois níveis de uma vez, e o jogador precisa escolher uma carta para cada.
    /// </summary>
    public sealed class Experience
    {
        private readonly int firstLevelCost;
        private readonly int costIncrease;

        public int Level { get; private set; } = 1;

        /// <summary>XP acumulado dentro do nível atual.</summary>
        public int Current { get; private set; }

        /// <summary>XP necessário para sair do nível atual.</summary>
        public int Required => firstLevelCost + ((Level - 1) * costIncrease);

        public int PendingLevelUps { get; private set; }

        public Experience(int firstLevelCost = 5, int costIncrease = 3)
        {
            if (firstLevelCost <= 0)
                throw new ArgumentOutOfRangeException(nameof(firstLevelCost), firstLevelCost, "O primeiro nível precisa custar algo.");
            if (costIncrease < 0)
                throw new ArgumentOutOfRangeException(nameof(costIncrease), costIncrease, "O custo não pode diminuir.");

            this.firstLevelCost = firstLevelCost;
            this.costIncrease = costIncrease;
        }

        public void Add(int amount)
        {
            if (amount < 0)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "XP não pode ser negativo.");

            Current += amount;

            while (Current >= Required)
            {
                Current -= Required;
                Level++;
                PendingLevelUps++;
            }
        }

        public bool TryConsumeLevelUp()
        {
            if (PendingLevelUps == 0)
                return false;

            PendingLevelUps--;
            return true;
        }
    }
}
