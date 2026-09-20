using ScriptableSurvivors.Domain;
using UnityEngine;
using Numerics = System.Numerics;
using Random = System.Random;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// Adaptador do spawner. Pergunta ao domínio QUANDO e ONDE nascer,
    /// sorteia um tipo do catálogo e monta o corpo visual.
    ///
    /// Não tem Update próprio: é o ArenaRunner que o chama, para a ordem
    /// entre nascer e simular ser explícita em vez de sorteada pela Unity.
    /// </summary>
    public sealed class EnemySpawner : MonoBehaviour
    {
        private EnemyData[] catalog;
        private Arena arena;
        private SpawnRing ring;
        private SpawnTimer timer;
        private Random random;

        private Material primitiveMaterial;

        public void Bind(
            EnemyData[] enemyCatalog,
            Arena boundArena,
            SpawnRing spawnRing,
            SpawnTimer spawnTimer,
            Random rng,
            Material baseMaterial)
        {
            catalog = enemyCatalog;
            primitiveMaterial = baseMaterial;
            arena = boundArena;
            ring = spawnRing;
            timer = spawnTimer;
            random = rng;
        }

        public void Advance(float deltaTime)
        {
            if (catalog == null || catalog.Length == 0)
                return;

            var due = timer.Advance(deltaTime);
            for (var i = 0; i < due; i++)
                Spawn();
        }

        private void Spawn()
        {
            var data = catalog[random.Next(catalog.Length)];
            if (data == null)
                return;

            var position = ring.NextPoint(arena.Player.Position, random);
            var enemy = new Enemy(data.ToDomain(), position);
            arena.Add(enemy);

            var body = CreateBody(data, enemy.Position);
            body.transform.SetParent(transform, worldPositionStays: true);
            body.AddComponent<EnemyView>().Bind(enemy);
        }

        private GameObject CreateBody(EnemyData data, Numerics.Vector2 position)
        {
            // Enquanto o campo Prefab estiver vazio, uma cápsula serve. No Dia 4
            // basta arrastar um modelo do Quaternius no asset — sem tocar aqui.
            var body = data.Prefab != null
                ? Instantiate(data.Prefab)
                : RuntimePrimitives.Create(PrimitiveType.Capsule, data.name, new Color(0.78f, 0.27f, 0.30f), primitiveMaterial);

            body.name = data.name;
            body.transform.position = new Vector3(position.X, 1f, position.Y);
            return body;
        }
    }
}
