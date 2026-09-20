using ScriptableSurvivors.Domain.Enemies;
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

        [Tooltip("Ajuste fino do tamanho do modelo. Não afeta regra nenhuma — "
               + "alcance e colisão continuam vindo dos números acima.")]
        [SerializeField, Min(0.01f)] private float modelScale = 1f;

        [Tooltip("Clipe tocado em loop enquanto o inimigo se move. Os modelos do "
               + "Quaternius trazem Walk, Run, Idle, Jump, Bite_Front, Death, "
               + "Dance e HitRecieve. Vazio não anima.")]
        [SerializeField] private string walkClipName = "Walk";

        [Tooltip("Clipe tocado uma vez ao levar dano, voltando ao de andar em "
               + "seguida. Nos modelos do Quaternius chama-se HitRecieve (sic). "
               + "Vazio não reage ao golpe.")]
        [SerializeField] private string hitClipName = "HitRecieve";

        [Header("Atributos")]
        [SerializeField, Min(0.1f)] private float maxHealth = 10f;
        [SerializeField, Min(0f)] private float speed = 2.5f;
        [SerializeField, Min(0f)] private float damage = 5f;
        [SerializeField, Min(0)] private int xpReward = 1;

        public GameObject Prefab => prefab;
        public float ModelScale => modelScale;
        public string WalkClipName => walkClipName;
        public string HitClipName => hitClipName;

        /// <summary>
        /// A fronteira. Devolve uma cópia dos valores; o domínio nunca recebe
        /// este objeto.
        /// </summary>
        public EnemyStats ToDomain() => new EnemyStats(maxHealth, speed, damage, xpReward, name);
    }
}
