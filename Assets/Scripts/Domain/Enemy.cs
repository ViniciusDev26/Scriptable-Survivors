using System.Numerics;

namespace ScriptableSurvivors.Domain
{
    /// <summary>
    /// Um inimigo vivo na arena. Os números vêm do catálogo e são
    /// compartilhados por todos do mesmo tipo; a vida e a posição são desta
    /// instância e de mais ninguém.
    /// </summary>
    public sealed class Enemy
    {
        public EnemyStats Stats { get; }
        public Health Health { get; }
        public Vector2 Position { get; }

        public Enemy(EnemyStats stats, Vector2 position)
        {
            Stats = stats;
            Health = stats.CreateHealth();
            Position = position;
        }
    }
}
