using ScriptableSurvivors.Domain;
using UnityEngine;
using UnityEngine.InputSystem;
using Numerics = System.Numerics;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// Adaptador do jogador: lê o teclado, entrega ao domínio, desenha o
    /// resultado. Nenhuma regra mora aqui — nem a velocidade, nem a rotação
    /// do input em relação à câmera.
    ///
    /// O apelido "Numerics" existe para deixar a fronteira visível: o vetor do
    /// domínio é System.Numerics.Vector2, não o UnityEngine.Vector2.
    /// </summary>
    public sealed class PlayerView : MonoBehaviour
    {
        private PlayerMovement movement;
        private float cameraYaw;

        public void Bind(PlayerMovement playerMovement, float yawDegrees)
        {
            movement = playerMovement;
            cameraYaw = yawDegrees;
        }

        private void Update()
        {
            if (movement == null)
                return;

            movement.Move(ReadKeyboard(), cameraYaw, Time.deltaTime);

            // O domínio raciocina no plano; a altura é assunto do visual.
            var position = movement.Position;
            transform.position = new Vector3(position.X, transform.position.y, position.Y);
        }

        private static Numerics.Vector2 ReadKeyboard()
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
