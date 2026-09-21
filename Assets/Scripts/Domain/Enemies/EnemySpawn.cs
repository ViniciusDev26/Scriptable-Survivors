using System;
using System.Collections.Generic;
using System.Numerics;

namespace ScriptableSurvivors.Domain.Enemies
{
    /// <summary>
    /// De onde vêm os inimigos: o catálogo e o círculo. QUANDO nascem é da
    /// WaveSchedule — aqui só se responde quem e onde.
    ///
    /// Isto vive dentro da Arena, e não no adaptador, porque nascer é parte da
    /// simulação. Se ficasse do lado da Unity, o spawner continuaria trabalhando
    /// durante a pausa de escolha de carta — e o jogador voltaria de uma tela
    /// parada para um cerco que se formou sem ele ver.
    /// </summary>
    public sealed class EnemySpawn
    {
        private readonly List<EnemyStats> catalog;
        private readonly SpawnRing ring;
        private readonly Random random;

        public EnemySpawn(IEnumerable<EnemyStats> enemyCatalog, SpawnRing spawnRing, Random rng)
        {
            if (enemyCatalog == null)
                throw new ArgumentNullException(nameof(enemyCatalog));

            catalog = new List<EnemyStats>(enemyCatalog);
            if (catalog.Count == 0)
                throw new ArgumentException("O catálogo de inimigos não pode estar vazio.", nameof(enemyCatalog));

            ring = spawnRing ?? throw new ArgumentNullException(nameof(spawnRing));
            random = rng ?? throw new ArgumentNullException(nameof(rng));
        }

        public Enemy Create(Vector2 center, float strength = 1f)
        {
            var stats = catalog[random.Next(catalog.Count)];
            return new Enemy(stats.Scaled(strength), ring.NextPoint(center, random));
        }
    }
}
