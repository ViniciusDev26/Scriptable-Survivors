using ScriptableSurvivors.Domain.Combat;
using ScriptableSurvivors.Domain.Enemies;
using UnityEngine;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// Desenha um inimigo na posição que o domínio calculou. Espelha o
    /// PlayerView — e, como ele, não decide nada.
    /// </summary>
    public sealed class EnemyView : MonoBehaviour
    {
        public Enemy Enemy { get; private set; }

        public void Bind(Enemy enemy)
        {
            Enemy = enemy;
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
        }

        private void SyncPosition()
        {
            var position = Enemy.Position;
            transform.position = new Vector3(position.X, transform.position.y, position.Y);
        }
    }
}
