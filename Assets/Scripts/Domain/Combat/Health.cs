using System;

namespace ScriptableSurvivors.Domain.Combat
{
    /// <summary>
    /// Vida de uma entidade viva. Cada inimigo em cena tem a sua — o valor
    /// máximo vem do catálogo (EnemyData), o valor atual pertence à instância.
    /// </summary>
    public sealed class Health
    {
        public float Max { get; private set; }
        public float Current { get; private set; }

        public bool IsDead => Current <= 0f;

        /// <summary>
        /// Disparado quando dano é efetivamente aplicado, com o quanto foi
        /// tirado de fato. Golpe em quem já morreu não dispara nada.
        ///
        /// É o evento que o corpo escuta para se contorcer — mesmo desenho
        /// de ProjectileFired e EnemySpawned.
        /// </summary>
        public event Action<float> Damaged;

        public Health(float max)
        {
            if (max <= 0f)
                throw new ArgumentOutOfRangeException(nameof(max), max, "Vida máxima deve ser positiva.");

            Max = max;
            Current = max;
        }

        public void TakeDamage(float amount)
        {
            if (amount < 0f)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Dano não pode ser negativo.");

            var before = Current;
            Current = Math.Max(0f, Current - amount);

            var applied = before - Current;
            if (applied > 0f)
                Damaged?.Invoke(applied);
        }

        /// <summary>
        /// Aumenta o teto e cura o mesmo tanto. Só aumentar o máximo deixaria o
        /// jogador com uma barra maior e vazia — o upgrade pareceria uma punição.
        /// </summary>
        public void RaiseMax(float amount)
        {
            if (amount < 0f)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "O aumento não pode ser negativo.");

            Max += amount;
            Current += amount;
        }

        public void Heal(float amount)
        {
            if (amount < 0f)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Cura não pode ser negativa.");

            Current = Math.Min(Max, Current + amount);
        }
    }
}
