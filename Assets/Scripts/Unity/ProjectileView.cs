using ScriptableSurvivors.Domain;
using UnityEngine;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// Desenha um tiro em voo e some quando o domínio o dá por gasto — por
    /// ter acertado alguém ou por ter esgotado o alcance.
    /// </summary>
    public sealed class ProjectileView : MonoBehaviour
    {
        private Projectile projectile;
        private float height;

        public void Bind(Projectile boundProjectile, float flightHeight)
        {
            projectile = boundProjectile;
            height = flightHeight;
            SyncPosition();
        }

        private void LateUpdate()
        {
            if (projectile == null)
                return;

            if (projectile.IsSpent)
            {
                Destroy(gameObject);
                return;
            }

            SyncPosition();
        }

        private void SyncPosition()
        {
            var position = projectile.Position;
            transform.position = new Vector3(position.X, height, position.Y);
        }
    }
}
