using ScriptableSurvivors.Domain;
using UnityEngine;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// Catálogo de um tipo de inimigo. Existe como arquivo .asset no projeto,
    /// criado por Assets → Create → Game → Enemy — sem escrever código.
    ///
    /// Guarda o que o TIPO é, nunca o que uma instância está sofrendo agora.
    /// Um campo mutável aqui seria compartilhado por todos os inimigos deste
    /// tipo e sobreviveria ao fim do Play.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Enemy", fileName = "NewEnemy")]
    public sealed class EnemyData : ScriptableObject
    {
        [Header("Visual")]
        [SerializeField] private GameObject prefab;

        [Header("Atributos")]
        [SerializeField, Min(0.1f)] private float maxHealth = 10f;
        [SerializeField, Min(0f)] private float speed = 2.5f;
        [SerializeField, Min(0f)] private float damage = 5f;
        [SerializeField, Min(0)] private int xpReward = 1;

        public GameObject Prefab => prefab;

        /// <summary>
        /// A fronteira. Devolve uma cópia dos valores; o domínio nunca recebe
        /// este objeto.
        /// </summary>
        public EnemyStats ToDomain() => new EnemyStats(maxHealth, speed, damage, xpReward, name);
    }
}
