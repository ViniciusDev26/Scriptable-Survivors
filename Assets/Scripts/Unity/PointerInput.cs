using UnityEngine;
using UnityEngine.InputSystem;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// Toque e mouse lidos pela mesma porta. O toque tem prioridade quando
    /// existe; o mouse serve para testar no desktop sem precisar de aparelho.
    ///
    /// Isto evita EventSystem, GraphicRaycaster e módulo de input com action
    /// maps — três peças que só existiriam para responder "onde o dedo está".
    /// </summary>
    internal static class PointerInput
    {
        public static bool Held => TouchHeld || MouseHeld;

        public static bool PressedThisFrame => TouchPressed || MousePressed;

        public static Vector2 Position => TouchHeld || TouchPressed ? TouchPosition : MousePosition;

        private static Touchscreen Touch => Touchscreen.current;

        private static bool TouchHeld =>
            Touch != null && Touch.primaryTouch.press.isPressed;

        private static bool TouchPressed =>
            Touch != null && Touch.primaryTouch.press.wasPressedThisFrame;

        private static Vector2 TouchPosition =>
            Touch == null ? Vector2.zero : Touch.primaryTouch.position.ReadValue();

        private static bool MouseHeld =>
            Mouse.current != null && Mouse.current.leftButton.isPressed;

        private static bool MousePressed =>
            Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        private static Vector2 MousePosition =>
            Mouse.current == null ? Vector2.zero : Mouse.current.position.ReadValue();
    }
}
