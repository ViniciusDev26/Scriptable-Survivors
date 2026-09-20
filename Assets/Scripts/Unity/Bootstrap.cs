using ScriptableSurvivors.Domain;
using ScriptableSurvivors.Domain.Combat;
using ScriptableSurvivors.Domain.Enemies;
using ScriptableSurvivors.Domain.Players;
using ScriptableSurvivors.Domain.Progression;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// Composition root. É o único objeto montado à mão na cena — câmera,
    /// chão, jogador, inimigos e interface nascem aqui, por código.
    ///
    /// As regras da partida vêm todas de um RunConfig. O que sobrou neste
    /// componente é apresentação: enquadramento, material e tamanho do chão.
    /// </summary>
    public sealed class Bootstrap : MonoBehaviour
    {
        [Header("Partida")]
        [Tooltip("Trocar este asset troca a run inteira, sem recompilar nada.")]
        [SerializeField] private RunConfig config;

        [Header("Visual")]
        [Tooltip("Material base das primitivas. Precisa ser um asset: shader sem referência é descartado da build e vira magenta.")]
        [SerializeField] private Material primitiveMaterial;

        [Tooltip("Altura em que o tiro voa. Só visual — o domínio raciocina no plano.")]
        [SerializeField] private float projectileHeight = 1f;

        [SerializeField, Min(1f)] private float groundRadius = 30f;

        [Header("Câmera")]
        [SerializeField] private float cameraYaw = 45f;
        [SerializeField] private float cameraPitch = 45f;
        [SerializeField] private float cameraDistance = 25f;
        [SerializeField] private float cameraSize = 10f;

        private readonly Dictionary<Weapon, WeaponData> weaponSources =
            new Dictionary<Weapon, WeaponData>();

        private void Awake()
        {
            if (config == null)
            {
                Debug.LogError(
                    "Bootstrap: arraste um RunConfig no campo Config. Sem ele não há partida.", this);
                return;
            }

            if (primitiveMaterial == null)
            {
                Debug.LogError(
                    "Bootstrap: arraste o asset Assets/Art/Materials/RuntimePrimitive " +
                    "no campo Primitive Material.", this);
                return;
            }

            var loadout = BuildLoadout();
            if (loadout.Count == 0)
            {
                Debug.LogError(
                    "Bootstrap: nenhuma arma no RunConfig. Arraste ao menos um WeaponData.", this);
                return;
            }

            ConfigureCamera();
            CreateGround();

            var arena = new Arena(
                config.CreatePlayer(),
                loadout,
                config.ContactRadius,
                config.HitRadius,
                config.CreateExperience(),
                BuildEnemySpawn());

            CreatePlayerBody(arena.Player);
            CreateProjectileFactory(arena);
            CreateEnemyBodyFactory(arena);
            CreateHud(arena);
            CreateRunner(arena);
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
                PrimitiveType.Plane, "Ground", new Color(0.16f, 0.18f, 0.22f), primitiveMaterial);

            // A primitiva Plane tem 10x10 unidades na escala 1.
            ground.transform.localScale = Vector3.one * (groundRadius / 5f);
        }

        private void CreatePlayerBody(Player player)
        {
            var body = RuntimePrimitives.Create(
                PrimitiveType.Capsule, "Player", new Color(0.90f, 0.74f, 0.26f), primitiveMaterial);
            body.transform.position = new Vector3(0f, 1f, 0f);
            body.AddComponent<PlayerView>().Bind(player);
        }

        /// <summary>
        /// Constrói uma arma do domínio por asset e guarda de onde cada uma
        /// veio, para a fábrica de projéteis achar o prefab certo depois.
        /// </summary>
        private List<Weapon> BuildLoadout()
        {
            var loadout = new List<Weapon>();

            foreach (var data in config.Weapons)
            {
                if (data == null)
                    continue;

                var weapon = new Weapon(data.ToDomain());
                weaponSources[weapon] = data;
                loadout.Add(weapon);
            }

            return loadout;
        }

        /// <summary>
        /// Converte o catálogo de assets em números puros e entrega à Arena.
        /// Nascer é parte da simulação, então a pausa de escolha de carta
        /// congela o spawn de graça.
        /// </summary>
        private EnemySpawn BuildEnemySpawn()
        {
            var stats = new List<EnemyStats>();

            foreach (var data in config.Enemies)
            {
                if (data != null)
                    stats.Add(data.ToDomain());
            }

            if (stats.Count == 0)
            {
                Debug.LogWarning(
                    "Bootstrap: catálogo de inimigos vazio no RunConfig — nada vai nascer.", this);
                return null;
            }

            return new EnemySpawn(
                stats,
                new SpawnRing(config.SpawnRadius),
                new SpawnTimer(config.SpawnInterval),
                config.CreateRandom());
        }

        private void CreateEnemyBodyFactory(Arena arena)
        {
            var host = new GameObject("Enemies");
            host.AddComponent<EnemyBodyFactory>().Bind(arena, config.Enemies, primitiveMaterial);
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
                        explosive ? new Color(0.95f, 0.55f, 0.18f) : new Color(0.98f, 0.92f, 0.45f),
                        primitiveMaterial);

                var scale = explosive ? Mathf.Max(0.5f, projectile.SplashRadius * 0.45f) : 0.35f;
                body.transform.localScale = Vector3.one * scale;
                body.transform.SetParent(host.transform, worldPositionStays: true);
                body.AddComponent<ProjectileView>().Bind(projectile, projectileHeight);
            };
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
            rect.sizeDelta = new Vector2(520f, 400f);

            canvasObject.AddComponent<HudView>().Bind(arena, label);

            CreateUpgradeScreen(arena, canvasObject.transform, label.font);
        }

        private void CreateUpgradeScreen(Arena arena, Transform canvas, Font font)
        {
            var upgrades = new List<Upgrade>();

            foreach (var data in config.UpgradePool)
            {
                if (data != null)
                    upgrades.Add(data.ToDomain());
            }

            if (upgrades.Count == 0)
            {
                Debug.LogError(
                    "Bootstrap: monte de upgrades vazio no RunConfig. Sem cartas, a run trava " +
                    "no primeiro nível — a Arena pausa esperando uma escolha que nunca vem.", this);
                return;
            }

            var host = new GameObject("UpgradeScreen");
            host.AddComponent<UpgradeScreen>().Bind(
                arena, new UpgradePool(upgrades), config.CreateRandom(),
                config.UpgradePool, font, canvas);
        }

        private void CreateRunner(Arena arena)
        {
            var host = new GameObject("ArenaRunner");
            host.AddComponent<ArenaRunner>().Bind(arena, cameraYaw);
        }

        private static Font LoadBuiltinFont()
        {
            var font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (font == null)
                Debug.LogError("Bootstrap: fonte embutida do HUD não encontrada.");

            return font;
        }
    }
}
