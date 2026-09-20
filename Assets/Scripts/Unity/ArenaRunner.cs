using ScriptableSurvivors.Domain;
using UnityEngine;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// O único Update do jogo: um passo de simulação por quadro. Nascer,
    /// perseguir, atirar e morrer acontecem todos dentro do Tick, em ordem
    /// definida pelo domínio — inclusive a pausa que congela tudo isso.
    ///
    /// As views desenham em LateUpdate, que a Unity garante rodar depois de
    /// TODOS os Update — é o que impede de desenhar meio quadro desatualizado.
    /// </summary>
    public sealed class ArenaRunner : MonoBehaviour
    {
        private Arena arena;
        private float cameraYaw;

        public void Bind(Arena boundArena, float yawDegrees)
        {
            arena = boundArena;
            cameraYaw = yawDegrees;
        }

        private void Update()
        {
            if (arena == null)
                return;

            arena.Tick(KeyboardInput.ReadMoveAxis(), cameraYaw, Time.deltaTime);
        }
    }
}
