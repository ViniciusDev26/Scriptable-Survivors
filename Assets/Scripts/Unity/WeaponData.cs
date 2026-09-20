using ScriptableSurvivors.Domain.Combat;
using UnityEngine;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// Catálogo de uma arma. Existe como arquivo .asset, criado por
    /// Assets → Create → Game → Weapon — sem escrever código.
    ///
    /// Como o EnemyData, guarda o que a arma É, nunca o estado de um disparo.
    /// O tempo de recarga corrente vive no Weapon do domínio, por instância.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Weapon", fileName = "NewWeapon")]
    public sealed class WeaponData : ScriptableObject
    {
        [Header("Visual")]
        [SerializeField] private GameObject projectilePrefab;

        [Header("Atributos")]
        [SerializeField, Min(0f)] private float damage = 10f;

        [Tooltip("Tiros por segundo.")]
        [SerializeField, Min(0.05f)] private float shotsPerSecond = 2f;

        [SerializeField, Min(0.1f)] private float projectileSpeed = 20f;

        [Tooltip("Distância máxima do tiro, e também o alcance da mira automática.")]
        [SerializeField, Min(0.5f)] private float range = 12f;

        [Tooltip("Raio de dano em área no impacto. Zero é arma de alvo único.")]
        [SerializeField, Min(0f)] private float splashRadius;

        public GameObject ProjectilePrefab => projectilePrefab;

        public WeaponStats ToDomain() =>
            new WeaponStats(damage, shotsPerSecond, projectileSpeed, range, splashRadius);
    }
}
