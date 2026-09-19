using System;
using ScriptableSurvivors.Domain;
using UnityEngine;

// Existem dois Random no escopo. O do .NET aceita semente; o da Unity é
// estático e global, sem semente injetável — inútil para run reproduzível.
using Random = System.Random;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// Composition root. É o único objeto montado à mão na cena — câmera,
    /// chão, jogador e spawner nascem aqui, por código.
    ///
    /// Estes campos migram para o RunConfig (ScriptableObject) no Dia 3.
    /// </summary>
    public sealed class Bootstrap : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField, Min(0.1f)] private float playerSpeed = 8f;

        [Header("Camera")]
        [SerializeField] private float cameraYaw = 45f;
        [SerializeField] private float cameraPitch = 45f;
        [SerializeField] private float cameraDistance = 25f;
        [SerializeField] private float cameraSize = 10f;

        [Header("Arena")]
        [SerializeField, Min(1f)] private float arenaRadius = 30f;

        [Header("Inimigos")]
        [SerializeField] private EnemyData[] enemyCatalog;
        [SerializeField, Min(1f)] private float spawnRadius = 25f;
        [SerializeField, Min(0.05f)] private float spawnInterval = 1f;

        [Tooltip("0 sorteia uma semente nova a cada run. Qualquer outro valor repete a mesma run.")]
        [SerializeField] private int randomSeed;

        private void Awake()
        {
            ConfigureCamera();
            CreateGround();

            var arena = new Arena(new PlayerMovement(playerSpeed));
            CreatePlayerBody(arena.Player);

            var spawner = CreateSpawner(arena);
            CreateRunner(arena, spawner);
        }

        private void ConfigureCamera()
        {
            var mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogError("Bootstrap: nenhuma câmera com a tag MainCamera na cena.", this);
                return;
            }

            mainCamera.orthographic = true;
            mainCamera.orthographicSize = cameraSize;
            mainCamera.transform.rotation = Quaternion.Euler(cameraPitch, cameraYaw, 0f);

            // Recua ao longo do próprio olhar, para enquadrar a origem.
            mainCamera.transform.position = -mainCamera.transform.forward * cameraDistance;
        }

        private void CreateGround()
        {
            var ground = RuntimePrimitives.Create(
                PrimitiveType.Plane, "Ground", new Color(0.16f, 0.18f, 0.22f));

            // A primitiva Plane tem 10x10 unidades na escala 1.
            ground.transform.localScale = Vector3.one * (arenaRadius / 5f);
        }

        private void CreatePlayerBody(PlayerMovement movement)
        {
            var body = RuntimePrimitives.Create(
                PrimitiveType.Capsule, "Player", new Color(0.90f, 0.74f, 0.26f));
            body.transform.position = new Vector3(0f, 1f, 0f);
            body.AddComponent<PlayerView>().Bind(movement);
        }

        private EnemySpawner CreateSpawner(Arena arena)
        {
            if (enemyCatalog == null || enemyCatalog.Length == 0)
            {
                Debug.LogWarning(
                    "Bootstrap: catálogo de inimigos vazio — nada vai nascer. " +
                    "Arraste um EnemyData no campo Enemy Catalog.", this);
                return null;
            }

            var host = new GameObject("Enemies");
            var spawner = host.AddComponent<EnemySpawner>();
            spawner.Bind(
                enemyCatalog,
                arena,
                new SpawnRing(spawnRadius),
                new SpawnTimer(spawnInterval),
                CreateRandom());
            return spawner;
        }

        private void CreateRunner(Arena arena, EnemySpawner spawner)
        {
            var host = new GameObject("ArenaRunner");
            host.AddComponent<ArenaRunner>().Bind(arena, spawner, cameraYaw);
        }

        private Random CreateRandom()
        {
            var seed = randomSeed != 0 ? randomSeed : Environment.TickCount;

            // Anotada no Console para que uma run interessante possa ser repetida.
            Debug.Log($"Semente desta run: {seed}");
            return new Random(seed);
        }
    }
}
