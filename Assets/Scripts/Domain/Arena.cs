using System;
using System.Collections.Generic;
using System.Numerics;

namespace ScriptableSurvivors.Domain
{
    /// <summary>
    /// A simulação inteira de uma run: o jogador, os inimigos vivos, e um
    /// único passo de tempo que os atualiza em ordem definida.
    ///
    /// A ordem dentro de Tick É a regra. Se os inimigos andassem antes do
    /// jogador, perseguiriam a posição do quadro anterior — um quadro de
    /// atraso, invisível a 60 fps e escancarado quando o jogo engasga.
    /// </summary>
    public sealed class Arena
    {
        private readonly List<Enemy> enemies = new List<Enemy>();

        public PlayerMovement Player { get; }
        public IReadOnlyList<Enemy> Enemies => enemies;

        public Arena(PlayerMovement player)
        {
            Player = player ?? throw new ArgumentNullException(nameof(player));
        }

        public void Add(Enemy enemy)
        {
            if (enemy == null)
                throw new ArgumentNullException(nameof(enemy));

            enemies.Add(enemy);
        }

        public void Tick(Vector2 playerInput, float cameraYawDegrees, float deltaTime)
        {
            if (deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime), deltaTime, "deltaTime não pode ser negativo.");

            Player.Move(playerInput, cameraYawDegrees, deltaTime);

            for (var i = 0; i < enemies.Count; i++)
                enemies[i].MoveToward(Player.Position, deltaTime);
        }
    }
}
