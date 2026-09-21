using System;
using ScriptableSurvivors.Domain.Combat;

namespace ScriptableSurvivors.Domain.Enemies
{
    /// <summary>
    /// Os números de um tipo de inimigo, já fora da Unity. É um retrato
    /// imutável do catálogo: o domínio recebe uma cópia dos valores, nunca a
    /// referência do asset, e por isso não tem como sujar o arquivo em disco.
    ///
    /// Note que a vida MÁXIMA está aqui, mas a vida ATUAL não — essa é estado
    /// de instância e mora em <see cref="Health"/>. Misturar as duas é o erro
    /// que a demonstração do Dia 4 provoca de propósito.
    /// </summary>
    public readonly struct EnemyStats
    {
        /// <summary>Vem do nome do arquivo. O adaptador usa para achar o prefab.</summary>
        public string Id { get; }

        public float MaxHealth { get; }
        public float Speed { get; }
        public float Damage { get; }
        public int XpReward { get; }

        public EnemyStats(float maxHealth, float speed, float damage, int xpReward, string id = "enemy")
        {
            if (maxHealth <= 0f)
                throw new ArgumentOutOfRangeException(nameof(maxHealth), maxHealth, "Vida máxima deve ser positiva.");
            if (speed < 0f)
                throw new ArgumentOutOfRangeException(nameof(speed), speed, "Velocidade não pode ser negativa.");
            if (damage < 0f)
                throw new ArgumentOutOfRangeException(nameof(damage), damage, "Dano não pode ser negativo.");
            if (xpReward < 0)
                throw new ArgumentOutOfRangeException(nameof(xpReward), xpReward, "XP não pode ser negativo.");

            Id = string.IsNullOrWhiteSpace(id) ? "enemy" : id;
            MaxHealth = maxHealth;
            Speed = speed;
            Damage = damage;
            XpReward = xpReward;
        }

        public Health CreateHealth() => new Health(MaxHealth);

        /// <summary>
        /// O mesmo inimigo, mais forte. Vida e dano multiplicam; velocidade e
        /// XP não.
        ///
        /// Velocidade escalada tornaria os rápidos impossíveis de evitar em
        /// poucas ondas. E XP escalado anularia o freio natural: inimigo mais
        /// duro demora mais para morrer, o que já reduz o ritmo de subir de
        /// nível — que é exatamente o crescimento que precisa desacelerar.
        /// </summary>
        public EnemyStats Scaled(float multiplier)
        {
            if (multiplier <= 0f)
                throw new ArgumentOutOfRangeException(nameof(multiplier), multiplier, "A força deve ser positiva.");

            return new EnemyStats(MaxHealth * multiplier, Speed, Damage * multiplier, XpReward, Id);
        }
    }
}
