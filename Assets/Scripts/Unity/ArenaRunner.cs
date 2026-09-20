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
        private readonly TouchStick stick = new TouchStick();

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

            // O dedo só comanda quando a partida está rodando: com a tela de
            // cartas ou a de morte abertas, o toque pertence a elas.
            stick.Read(accepting: !arena.IsOver && !arena.IsAwaitingUpgrade);

            var keys = KeyboardInput.ReadMoveAxis();
            var input = keys == System.Numerics.Vector2.Zero ? stick.Direction : keys;

            arena.Tick(input, cameraYaw, Time.deltaTime);
        }
    }
}
