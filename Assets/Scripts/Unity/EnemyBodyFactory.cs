using System.Collections.Generic;
using ScriptableSurvivors.Domain;
using ScriptableSurvivors.Domain.Enemies;
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
            var usingModel = data != null && data.Prefab != null;

            // Enquanto o campo Prefab estiver vazio, uma cápsula serve.
            var body = usingModel
                ? Instantiate(data.Prefab)
                : RuntimePrimitives.Create(
                    PrimitiveType.Capsule, enemy.Stats.Id, new Color(0.78f, 0.27f, 0.30f), primitiveMaterial);

            // A cápsula tem pivô no centro e mede 2 de altura, então nasce em
            // y = 1 para a base encostar no chão. Modelo tem pivô nos pés, e a
            // altura vem do catálogo — zero para quem anda, maior para voadores.
            var height = usingModel ? data.BodyHeight : 1f;

            if (usingModel)
            {
                body.transform.localScale = Vector3.one * data.ModelScale;

                var clips = body.GetComponentInChildren<Animation>();
                if (clips != null)
                {
                    body.AddComponent<EnemyAnimator>()
                        .Bind(enemy, clips, data.WalkClipName, data.HitClipName);
                }
            }

            body.name = enemy.Stats.Id;
            body.transform.position = new Vector3(enemy.Position.X, height, enemy.Position.Y);
            body.transform.SetParent(transform, worldPositionStays: true);
            body.AddComponent<EnemyView>().Bind(enemy);
        }
    }
}
