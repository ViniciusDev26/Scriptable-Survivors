using ScriptableSurvivors.Domain;
using UnityEngine;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// Desenha o jogador na posição que o domínio calculou. Só isso — quem
    /// lê o teclado é o ArenaRunner, e quem decide para onde ir é o domínio.
    /// </summary>
    public sealed class PlayerView : MonoBehaviour
    {
        private Player movement;

        public void Bind(Player playerMovement)
        {
            movement = playerMovement;
            SyncPosition();
        }

        private void LateUpdate()
        {
            if (movement != null)
                SyncPosition();
        }

        private void SyncPosition()
        {
            // O domínio raciocina no plano; a altura é assunto do visual.
            var position = movement.Position;
            transform.position = new Vector3(position.X, transform.position.y, position.Y);
        }
    }
}
