using ScriptableSurvivors.Domain;
using UnityEngine;
using UnityEngine.Rendering;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// Composition root. É o único objeto montado à mão na cena — câmera,
    /// chão e jogador nascem aqui, por código.
    ///
    /// Estes campos migram para o RunConfig (ScriptableObject) no Dia 2.
    /// </summary>
    public sealed class Bootstrap : MonoBehaviour
    {
        [Header("Player")]
        [SerializeField] private float playerSpeed = 8f;

        [Header("Camera")]
        [SerializeField] private float cameraYaw = 45f;
        [SerializeField] private float cameraPitch = 45f;
        [SerializeField] private float cameraDistance = 25f;
        [SerializeField] private float cameraSize = 10f;

        [Header("Arena")]
        [SerializeField] private float arenaRadius = 30f;

        private void Awake()
        {
            ConfigureCamera();
            CreateGround();
            CreatePlayer();
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
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";

            // A primitiva Plane tem 10x10 unidades na escala 1.
            ground.transform.localScale = Vector3.one * (arenaRadius / 5f);
            ground.GetComponent<Renderer>().sharedMaterial =
                CreateMaterial(new Color(0.16f, 0.18f, 0.22f));
        }

        private void CreatePlayer()
        {
            var player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = "Player";
            player.transform.position = new Vector3(0f, 1f, 0f);
            player.GetComponent<Renderer>().sharedMaterial =
                CreateMaterial(new Color(0.90f, 0.74f, 0.26f));

            player.AddComponent<PlayerView>()
                  .Bind(new PlayerMovement(playerSpeed), cameraYaw);
        }

        /// <summary>
        /// Primitivas criadas em runtime vêm com o material do pipeline antigo,
        /// que a URP desenha em rosa. Este clona o material padrão do pipeline ativo.
        /// </summary>
        private static Material CreateMaterial(Color color)
        {
            var pipeline = GraphicsSettings.currentRenderPipeline;
            var material = pipeline != null && pipeline.defaultMaterial != null
                ? new Material(pipeline.defaultMaterial)
                : new Material(Shader.Find("Universal Render Pipeline/Lit"));

            material.color = color;
            return material;
        }
    }
}
