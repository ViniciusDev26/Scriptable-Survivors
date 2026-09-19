using UnityEngine;
using UnityEngine.Rendering;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// Cubos e cápsulas coloridos, criados em runtime. Tudo aqui é provisório:
    /// no Dia 4 os modelos do Quaternius entram no lugar, pelo campo Prefab
    /// dos ScriptableObjects.
    /// </summary>
    internal static class RuntimePrimitives
    {
        public static GameObject Create(PrimitiveType type, string name, Color color)
        {
            var instance = GameObject.CreatePrimitive(type);
            instance.name = name;
            instance.GetComponent<Renderer>().sharedMaterial = CreateMaterial(color);

            // A colisão é resolvida no domínio, por distância. O colisor que vem
            // de brinde seria peso morto — e, pior, um colisor sem Rigidbody
            // movido a cada quadro faz a Unity reconstruir a árvore de colisão
            // estática de graça.
            var collider = instance.GetComponent<Collider>();
            if (collider != null)
                Object.Destroy(collider);

            return instance;
        }

        /// <summary>
        /// Primitivas criadas em runtime vêm com o material do pipeline antigo,
        /// que a URP desenha em rosa. Este clona o material padrão do pipeline
        /// ativo para herdar o shader certo.
        /// </summary>
        public static Material CreateMaterial(Color color)
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
