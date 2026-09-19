using ScriptableSurvivors.Domain;
using UnityEngine;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// O único Update do jogo. Aqui a ordem é explícita: primeiro nascem os
    /// inimigos do quadro, depois a simulação avança um passo.
    ///
    /// As views desenham em LateUpdate, que a Unity garante rodar depois de
    /// TODOS os Update — é o que impede de desenhar meio quadro desatualizado.
    /// </summary>
    public sealed class ArenaRunner : MonoBehaviour
    {
        private Arena arena;
        private EnemySpawner spawner;
        private float cameraYaw;

        public void Bind(Arena boundArena, EnemySpawner enemySpawner, float yawDegrees)
        {
            arena = boundArena;
            spawner = enemySpawner;
            cameraYaw = yawDegrees;
        }

        private void Update()
        {
            if (arena == null)
                return;

            var deltaTime = Time.deltaTime;

            if (spawner != null)
                spawner.Advance(deltaTime);

            arena.Tick(KeyboardInput.ReadMoveAxis(), cameraYaw, deltaTime);
        }
    }
}
