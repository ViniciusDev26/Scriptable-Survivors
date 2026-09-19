using System;
using System.Collections.Generic;
using ScriptableSurvivors.Domain;
using UnityEngine;
using UnityEngine.UI;

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
        [SerializeField, Min(1f)] private float playerMaxHealth = 100f;

        [Tooltip("Distância a partir da qual um inimigo encosta e começa a machucar.")]
        [SerializeField, Min(0.1f)] private float contactRadius = 1.2f;

        [Header("Camera")]
        [SerializeField] private float cameraYaw = 45f;
        [SerializeField] private float cameraPitch = 45f;
        [SerializeField] private float cameraDistance = 25f;
        [SerializeField] private float cameraSize = 10f;

        [Header("Arena")]
        [SerializeField, Min(1f)] private float arenaRadius = 30f;

        [Header("Armas")]
        [Tooltip("Todas disparam sozinhas, cada uma com sua própria cadência.")]
        [SerializeField] private WeaponData[] weapons;

        [Tooltip("Altura em que o tiro voa. Só visual — o domínio raciocina no plano.")]
        [SerializeField] private float projectileHeight = 1f;

        [Tooltip("Distância a partir da qual um tiro acerta um inimigo.")]
        [SerializeField, Min(0.1f)] private float hitRadius = 0.8f;

        [Header("Inimigos")]
        [SerializeField] private EnemyData[] enemyCatalog;
        [SerializeField, Min(1f)] private float spawnRadius = 25f;
        [SerializeField, Min(0.05f)] private float spawnInterval = 1f;

        [Tooltip("0 sorteia uma semente nova a cada run. Qualquer outro valor repete a mesma run.")]
        [SerializeField] private int randomSeed;

        private readonly Dictionary<Weapon, WeaponData> weaponSources =
            new Dictionary<Weapon, WeaponData>();

        private void Awake()
        {
            ConfigureCamera();
            CreateGround();

            var loadout = BuildLoadout();
            if (loadout.Count == 0)
            {
                Debug.LogError(
                    "Bootstrap: nenhuma arma. Arraste ao menos um WeaponData no campo Weapons.", this);
                return;
            }

            var arena = new Arena(
                new Player(playerSpeed, playerMaxHealth),
                loadout,
                contactRadius,
                hitRadius);

            CreatePlayerBody(arena.Player);
            CreateProjectileFactory(arena);
            CreateHud(arena);

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

        private void CreatePlayerBody(Player movement)
        {
            var body = RuntimePrimitives.Create(
                PrimitiveType.Capsule, "Player", new Color(0.90f, 0.74f, 0.26f));
            body.transform.position = new Vector3(0f, 1f, 0f);
            body.AddComponent<PlayerView>().Bind(movement);
        }

        /// <summary>
        /// Constrói uma arma do domínio por asset e guarda de onde cada uma
        /// veio, para a fábrica de projéteis achar o prefab certo depois.
        /// </summary>
        private List<Weapon> BuildLoadout()
        {
            var loadout = new List<Weapon>();
            if (weapons == null)
                return loadout;

            foreach (var data in weapons)
            {
                if (data == null)
                    continue;

                var weapon = new Weapon(data.ToDomain());
                weaponSources[weapon] = data;
                loadout.Add(weapon);
            }

            return loadout;
        }

        private void CreateHud(Arena arena)
        {
            var canvasObject = new GameObject("HUD", typeof(Canvas), typeof(CanvasScaler));
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            // Sem o scaler, o texto encolheria em telas grandes e sumiria no
            // canto durante a apresentação.
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);

            var labelObject = new GameObject("Stats", typeof(Text));
            labelObject.transform.SetParent(canvasObject.transform, worldPositionStays: false);

            var label = labelObject.GetComponent<Text>();
            label.font = LoadBuiltinFont();
            label.fontSize = 34;
            label.lineSpacing = 1.2f;
            label.color = Color.white;
            label.alignment = TextAnchor.UpperLeft;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;

            // Ancorado no canto superior esquerdo, para não depender da resolução.
            var rect = label.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(0f, 1f);
            rect.pivot = new Vector2(0f, 1f);
            rect.anchoredPosition = new Vector2(28f, -24f);
            rect.sizeDelta = new Vector2(480f, 140f);

            canvasObject.AddComponent<HudView>().Bind(arena, label);
        }

        private static Font LoadBuiltinFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                Debug.LogError("Bootstrap: fonte embutida do HUD não encontrada.");

            return font;
        }

        /// <summary>
        /// Assina o evento de domínio e dá corpo a cada tiro. É literalmente o
        /// "MonoBehaviour traduz evento de domínio em visual" do projeto.
        /// </summary>
        private void CreateProjectileFactory(Arena arena)
        {
            var host = new GameObject("Projectiles");

            arena.ProjectileFired += (weapon, projectile) =>
            {
                weaponSources.TryGetValue(weapon, out var data);
                var prefab = data != null ? data.ProjectilePrefab : null;

                // Tiro explosivo nasce maior e alaranjado, para a diferença
                // entre as armas ser visível sem precisar de explicação.
                var explosive = projectile.SplashRadius > 0f;
                var body = prefab != null
                    ? Instantiate(prefab)
                    : RuntimePrimitives.Create(
                        PrimitiveType.Sphere,
                        "Shot",
                        explosive ? new Color(0.95f, 0.55f, 0.18f) : new Color(0.98f, 0.92f, 0.45f));

                var scale = explosive ? Mathf.Max(0.5f, projectile.SplashRadius * 0.45f) : 0.35f;
                body.transform.localScale = Vector3.one * scale;
                body.transform.SetParent(host.transform, worldPositionStays: true);
                body.AddComponent<ProjectileView>().Bind(projectile, projectileHeight);
            };
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
