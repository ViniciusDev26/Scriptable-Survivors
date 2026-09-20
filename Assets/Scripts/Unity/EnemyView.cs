using ScriptableSurvivors.Domain.Combat;
using ScriptableSurvivors.Domain.Enemies;
using UnityEngine;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// Desenha um inimigo na posição que o domínio calculou, e o vira para
    /// onde ele está indo. Espelha o PlayerView — e, como ele, não decide nada.
    ///
    /// A direção é deduzida do deslocamento entre quadros em vez de vir do
    /// domínio: para onde o corpo aponta é decisão visual, e uma cápsula nem
    /// tem frente.
    /// </summary>
    public sealed class EnemyView : MonoBehaviour
    {
        private const float MinimumStepToTurn = 0.0001f;

        public Enemy Enemy { get; private set; }

        private Vector3 previousPosition;

        public void Bind(Enemy enemy)
        {
            Enemy = enemy;
            previousPosition = transform.position;
            SyncPosition();
        }

        private void LateUpdate()
        {
            if (Enemy == null)
                return;

            // A Arena já tirou este inimigo da simulação; resta sumir o corpo.
            if (Enemy.Health.IsDead)
            {
                Destroy(gameObject);
                return;
            }

            SyncPosition();
            FaceTheWay();
        }

        private void SyncPosition()
        {
            var position = Enemy.Position;
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
