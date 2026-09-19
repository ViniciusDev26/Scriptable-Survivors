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
