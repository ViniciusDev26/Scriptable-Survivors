using ScriptableSurvivors.Domain.Players;
using UnityEngine;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// Desenha o jogador na posição que o domínio calculou e o vira para onde
    /// ele está indo. Só isso — quem lê o teclado é o ArenaRunner, e quem
    /// decide para onde ir é o domínio.
    /// </summary>
    public sealed class PlayerView : MonoBehaviour
    {
        private const float MinimumStepToTurn = 0.0001f;

        private Player movement;
        private Vector3 previousPosition;

        public void Bind(Player playerMovement)
        {
            movement = playerMovement;
            previousPosition = transform.position;
            SyncPosition();
        }

        private void LateUpdate()
        {
            if (movement == null)
                return;

            SyncPosition();
            FaceTheWay();
        }

        private void SyncPosition()
        {
            // O domínio raciocina no plano; a altura é assunto do visual.
            var position = movement.Position;
            transform.position = new Vector3(position.X, transform.position.y, position.Y);
        }

        private void FaceTheWay()
        {
            var step = transform.position - previousPosition;
            step.y = 0f;

            if (step.sqrMagnitude > MinimumStepToTurn)
                transform.rotation = Quaternion.LookRotation(step, Vector3.up);

            previousPosition = transform.position;
        }
    }
}
