using ScriptableSurvivors.Domain;
using ScriptableSurvivors.Domain.Enemies;
using System.Collections.Generic;
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
        /// <summary>
        /// Os FBX são importados como Legacy, então trazem um componente
        /// Animation com os clipes prontos e sem precisar de Animator
        /// Controller — um por modelo, já que os clipes moram dentro do arquivo.
        ///
        /// O ganho decisivo do Legacy aqui é o wrapMode: clipes importados não
        /// vêm marcados como loop, e por código isso se resolve numa linha. Com
        /// Animator seria preciso configurar os nove clipes de cada modelo.
        /// </summary>
        private static void StartWalking(GameObject body, EnemyData data)
        {
            if (string.IsNullOrWhiteSpace(data.WalkClipName))
                return;

            var animation = body.GetComponentInChildren<Animation>();
            if (animation == null)
                return;

            var state = animation[data.WalkClipName];
            if (state == null)
            {
                Debug.LogWarning(
                    $"{data.name}: o modelo não tem o clipe '{data.WalkClipName}'.", data);
                return;
            }

            // O wrapMode precisa ir no ESTADO, não no componente: o do
            // componente só vale como padrão para estados criados depois, e os
            // do importador já existem. Sem isto o clipe toca uma vez e congela
            // na última pose — o monstro anda alguns passos e volta a deslizar.
            state.wrapMode = WrapMode.Loop;
            animation.wrapMode = WrapMode.Loop;
            animation.Play(data.WalkClipName);
        }

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
            var usingModel = data != null && data.Prefab != null;

            var body = usingModel
                ? Instantiate(data.Prefab)
                : RuntimePrimitives.Create(
                    PrimitiveType.Capsule, enemy.Stats.Id, new Color(0.78f, 0.27f, 0.30f), primitiveMaterial);

            // A cápsula tem pivô no centro e mede 2 de altura, então nasce em
            // y = 1 para a base encostar no chão. Modelo tem pivô nos pés — em
            // y = 1 ele flutuaria.
            var height = usingModel ? 0f : 1f;

            if (usingModel)
                body.transform.localScale = Vector3.one * data.ModelScale;

            if (usingModel)
                StartWalking(body, data);

            body.name = enemy.Stats.Id;
            body.transform.position = new Vector3(enemy.Position.X, height, enemy.Position.Y);
            body.transform.SetParent(transform, worldPositionStays: true);
            body.AddComponent<EnemyView>().Bind(enemy);
        }
    }
}
