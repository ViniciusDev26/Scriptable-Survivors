using System.Collections.Generic;
using ScriptableSurvivors.Domain;
using UnityEngine;
using Random = System.Random;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// Tudo o que define uma run, num arquivo só.
    ///
    /// É o ScriptableObject que referencia outros ScriptableObjects: o catálogo
    /// de inimigos, o arsenal inicial e o monte de upgrades são listas de
    /// assets, não de números. Trocar este arquivo no Bootstrap troca a
    /// partida inteira — e é o gesto que a apresentação vai mostrar.
    ///
    /// O que fica de fora daqui é o que é apresentação, não regra: câmera,
    /// material das primitivas e tamanho do chão continuam no Bootstrap.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Run Config", fileName = "NewRunConfig")]
    public sealed class RunConfig : ScriptableObject
    {
        [Header("Jogador")]
        [SerializeField, Min(0.1f)] private float playerSpeed = 8f;
        [SerializeField, Min(1f)] private float playerMaxHealth = 100f;

        [Header("Arsenal inicial")]
        [Tooltip("Todas disparam sozinhas, cada uma com sua própria cadência.")]
        [SerializeField] private WeaponData[] weapons;

        [Header("Inimigos")]
        [SerializeField] private EnemyData[] enemies;
        [SerializeField, Min(1f)] private float spawnRadius = 25f;

        [Tooltip("Segundos entre nascimentos.")]
        [SerializeField, Min(0.05f)] private float spawnInterval = 1f;

        [Header("Progressão")]
        [Tooltip("O monte de cartas. Criar um upgrade novo é criar um asset e arrastar aqui.")]
        [SerializeField] private UpgradeData[] upgradePool;

        [Tooltip("XP para sair do nível 1.")]
        [SerializeField, Min(1)] private int firstLevelCost = 5;

        [Tooltip("Quanto o custo do nível cresce a cada nível.")]
        [SerializeField, Min(0)] private int costIncrease = 3;

        [Header("Colisão")]
        [Tooltip("Distância a partir da qual um inimigo encosta e machuca.")]
        [SerializeField, Min(0.1f)] private float contactRadius = 1.2f;

        [Tooltip("Distância a partir da qual um tiro acerta.")]
        [SerializeField, Min(0.1f)] private float hitRadius = 0.8f;

        [Header("Sorteio")]
        [Tooltip("0 sorteia uma semente nova a cada run. Qualquer outro valor repete a mesma run.")]
        [SerializeField] private int randomSeed;

        public IReadOnlyList<WeaponData> Weapons => weapons;
        public IReadOnlyList<EnemyData> Enemies => enemies;
        public IReadOnlyList<UpgradeData> UpgradePool => upgradePool;

        public float ContactRadius => contactRadius;
        public float HitRadius => hitRadius;
        public float SpawnRadius => spawnRadius;
        public float SpawnInterval => spawnInterval;

        public Player CreatePlayer() => new Player(playerSpeed, playerMaxHealth);

        public Experience CreateExperience() => new Experience(firstLevelCost, costIncrease);

        /// <summary>
        /// A semente é anotada no Console para que uma run interessante possa
        /// ser repetida — basta copiar o número de volta para o campo.
        /// </summary>
        public Random CreateRandom()
        {
            var seed = randomSeed != 0 ? randomSeed : System.Environment.TickCount;
            Debug.Log($"Semente desta run: {seed}");
            return new Random(seed);
        }
    }
}
