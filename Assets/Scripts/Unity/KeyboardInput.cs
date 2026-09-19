using UnityEngine.InputSystem;
using Numerics = System.Numerics;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// Traduz teclas em um vetor de intenção. Não sabe o que "para cima"
    /// significa no mundo — isso é PlanarInput, no domínio.
    /// </summary>
    internal static class KeyboardInput
    {
        public static Numerics.Vector2 ReadMoveAxis()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return Numerics.Vector2.Zero;

            var horizontal = 0f;
            var vertical = 0f;

            if (keyboard.aKey.isPressed) horizontal -= 1f;
            if (keyboard.dKey.isPressed) horizontal += 1f;
            if (keyboard.sKey.isPressed) vertical -= 1f;
            if (keyboard.wKey.isPressed) vertical += 1f;

            return new Numerics.Vector2(horizontal, vertical);
        }
    }
}
