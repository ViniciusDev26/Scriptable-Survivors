using System;
using System.Collections.Generic;
using System.Numerics;

namespace ScriptableSurvivors.Domain.Enemies
{
    /// <summary>
    /// De onde vêm os inimigos: o catálogo, o relógio e o círculo, juntos.
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
        private readonly SpawnTimer timer;
        private readonly Random random;

        public EnemySpawn(IEnumerable<EnemyStats> enemyCatalog, SpawnRing spawnRing, SpawnTimer spawnTimer, Random rng)
        {
            if (enemyCatalog == null)
                throw new ArgumentNullException(nameof(enemyCatalog));

            catalog = new List<EnemyStats>(enemyCatalog);
            if (catalog.Count == 0)
                throw new ArgumentException("O catálogo de inimigos não pode estar vazio.", nameof(enemyCatalog));

            ring = spawnRing ?? throw new ArgumentNullException(nameof(spawnRing));
            timer = spawnTimer ?? throw new ArgumentNullException(nameof(spawnTimer));
            random = rng ?? throw new ArgumentNullException(nameof(rng));
        }

        public int Advance(float deltaTime) => timer.Advance(deltaTime);

        public Enemy Create(Vector2 center)
        {
            var stats = catalog[random.Next(catalog.Count)];
            return new Enemy(stats, ring.NextPoint(center, random));
        }
    }
}
