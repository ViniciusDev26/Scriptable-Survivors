using System.Collections.Generic;
using ScriptableSurvivors.Domain;
using UnityEngine;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// Dá corpo aos inimigos que o domínio fez nascer. Não decide quando nem
    /// onde — isso é EnemySpawn, dentro da Arena. Aqui só se traduz um evento
    /// de domínio em algo visível.
    /// </summary>
    public sealed class EnemyBodyFactory : MonoBehaviour
    {
        private readonly Dictionary<string, EnemyData> catalog = new Dictionary<string, EnemyData>();
        private Material primitiveMaterial;

        public void Bind(Arena arena, IEnumerable<EnemyData> sources, Material baseMaterial)
        {
            primitiveMaterial = baseMaterial;

            foreach (var data in sources)
            {
                if (data != null)
                    catalog[data.name] = data;
            }

            arena.EnemySpawned += GiveBody;
        }

        private void GiveBody(Enemy enemy)
        {
            catalog.TryGetValue(enemy.Stats.Id, out var data);

            // Enquanto o campo Prefab estiver vazio, uma cápsula serve. No Dia 4
            // basta arrastar um modelo do Quaternius no asset — sem tocar aqui.
            var body = data != null && data.Prefab != null
                ? Instantiate(data.Prefab)
                : RuntimePrimitives.Create(
                    PrimitiveType.Capsule, enemy.Stats.Id, new Color(0.78f, 0.27f, 0.30f), primitiveMaterial);

            body.name = enemy.Stats.Id;
            body.transform.position = new Vector3(enemy.Position.X, 1f, enemy.Position.Y);
            body.transform.SetParent(transform, worldPositionStays: true);
            body.AddComponent<EnemyView>().Bind(enemy);
        }
    }
}
