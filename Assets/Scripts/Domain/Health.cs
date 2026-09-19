using System;

namespace ScriptableSurvivors.Domain
{
    /// <summary>
    /// Vida de uma entidade viva. Cada inimigo em cena tem a sua — o valor
    /// máximo vem do catálogo (EnemyData), o valor atual pertence à instância.
    /// </summary>
    public sealed class Health
    {
        public float Max { get; }
        public float Current { get; private set; }

        public bool IsDead => Current <= 0f;

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

            Current = Math.Max(0f, Current - amount);
        }

        public void Heal(float amount)
        {
            if (amount < 0f)
                throw new ArgumentOutOfRangeException(nameof(amount), amount, "Cura não pode ser negativa.");

            Current = Math.Min(Max, Current + amount);
        }
    }
}
