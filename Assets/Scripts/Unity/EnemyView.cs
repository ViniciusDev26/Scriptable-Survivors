using ScriptableSurvivors.Domain;
using UnityEngine;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// Adaptador de um inimigo: copia a posição que o domínio calculou para o
    /// Transform. Espelha o PlayerView — e, como ele, não decide nada.
    /// </summary>
    public sealed class EnemyView : MonoBehaviour
    {
        public Enemy Enemy { get; private set; }

        public void Bind(Enemy enemy)
        {
            Enemy = enemy;
            SyncPosition();
        }

        private void Update()
        {
            if (Enemy != null)
                SyncPosition();
        }

        private void SyncPosition()
        {
            var position = Enemy.Position;
            transform.position = new Vector3(position.X, transform.position.y, position.Y);
        }
    }
}
