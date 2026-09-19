using System.Collections.Generic;
using ScriptableSurvivors.Domain;
using UnityEngine;
using Numerics = System.Numerics;
using Random = System.Random;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// Adaptador do spawner. Pergunta ao domínio QUANDO e ONDE nascer, sorteia
    /// um tipo do catálogo e monta o objeto visual. Nenhuma dessas contas é
    /// feita aqui.
    /// </summary>
    public sealed class EnemySpawner : MonoBehaviour
    {
        private readonly List<EnemyView> living = new List<EnemyView>();

        private EnemyData[] catalog;
        private PlayerMovement player;
        private SpawnRing ring;
        private SpawnTimer timer;
        private Random random;

        public IReadOnlyList<EnemyView> Living => living;

        public void Bind(
            EnemyData[] enemyCatalog,
            PlayerMovement trackedPlayer,
            SpawnRing spawnRing,
            SpawnTimer spawnTimer,
            Random rng)
        {
            catalog = enemyCatalog;
            player = trackedPlayer;
            ring = spawnRing;
            timer = spawnTimer;
            random = rng;
        }

        private void Update()
        {
            if (catalog == null || catalog.Length == 0)
                return;

            var due = timer.Advance(Time.deltaTime);
            for (var i = 0; i < due; i++)
                Spawn();
        }

        private void Spawn()
        {
            var data = catalog[random.Next(catalog.Length)];
            if (data == null)
                return;

            var position = ring.NextPoint(player.Position, random);
            var enemy = new Enemy(data.ToDomain(), position);

            var body = CreateBody(data, enemy.Position);
            body.transform.SetParent(transform, worldPositionStays: true);

            var view = body.AddComponent<EnemyView>();
            view.Bind(enemy);
            living.Add(view);
        }

        private GameObject CreateBody(EnemyData data, Numerics.Vector2 position)
        {
            // Enquanto o campo Prefab estiver vazio, uma cápsula serve. No Dia 4
            // basta arrastar um modelo do Quaternius no asset — sem tocar aqui.
            var body = data.Prefab != null
                ? Instantiate(data.Prefab)
                : RuntimePrimitives.Create(PrimitiveType.Capsule, data.name, new Color(0.78f, 0.27f, 0.30f));

            body.name = data.name;
            body.transform.position = new Vector3(position.X, 1f, position.Y);
            return body;
        }
    }
}
