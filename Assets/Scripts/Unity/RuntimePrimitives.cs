using UnityEngine;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// Cubos e cápsulas coloridos, criados em runtime. Tudo aqui é provisório:
    /// no Dia 4 os modelos do Quaternius entram no lugar, pelo campo Prefab
    /// dos ScriptableObjects.
    /// </summary>
    internal static class RuntimePrimitives
    {
        /// <summary>
        /// O material base vem de fora, como asset referenciado pela cena.
        ///
        /// Não use Shader.Find aqui. Nada na cena usa material — tudo nasce por
        /// código — então a Unity descarta os shaders sem referência ao montar a
        /// build, e o Shader.Find devolve null em runtime. O resultado é magenta,
        /// que aparece só na build e nunca no editor.
        /// </summary>
        public static GameObject Create(PrimitiveType type, string name, Color color, Material baseMaterial)
        {
            var instance = GameObject.CreatePrimitive(type);
            instance.name = name;
            instance.GetComponent<Renderer>().sharedMaterial = Tint(baseMaterial, color);

            // A colisão é resolvida no domínio, por distância. O colisor que vem
            // de brinde seria peso morto — e, pior, um colisor sem Rigidbody
            // movido a cada quadro faz a Unity reconstruir a árvore de colisão
            // estática de graça.
            var collider = instance.GetComponent<Collider>();
            if (collider != null)
                Object.Destroy(collider);

            return instance;
        }

        public static Material Tint(Material baseMaterial, Color color)
        {
            if (baseMaterial == null)
            {
                Debug.LogError(
                    "RuntimePrimitives: material base ausente. Arraste o asset " +
                    "RuntimePrimitive no campo Primitive Material do Bootstrap.");
                return null;
            }

            return new Material(baseMaterial) { color = color };
        }
    }
}
