using System;
using System.Collections.Generic;
using ScriptableSurvivors.Domain.Players;
using UnityEngine;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// Alterna entre parado e andando conforme o jogador se move.
    ///
    /// Os inimigos não precisam disto: eles perseguem sem descanso e nunca
    /// param. O jogador para o tempo todo, e um herói deslizando em pose de
    /// caminhada seria pior que a cápsula.
    ///
    /// O movimento é deduzido da posição entre quadros, como a orientação —
    /// o domínio raciocina em posições, não em poses.
    /// </summary>
    public sealed class PlayerAnimator : MonoBehaviour
    {
        private const float Blend = 0.12f;
        private const float StillThreshold = 0.0001f;

        private Player player;
        private Animation clips;
        private AnimationState walk;
        private AnimationState idle;

        private Vector2 previous;
        private bool wasMoving;

        public void Bind(Player boundPlayer, Animation animation, string walkClipName, string idleClipName)
        {
            player = boundPlayer;
            clips = animation;

            walk = Find(walkClipName);
            idle = Find(idleClipName);

            foreach (var state in new[] { walk, idle })
            {
                if (state != null)
                    state.wrapMode = WrapMode.Loop;
            }

            previous = new Vector2(player.Position.X, player.Position.Y);
            Switch(moving: false, instant: true);
        }

        private void LateUpdate()
        {
            if (player == null)
                return;

            var now = new Vector2(player.Position.X, player.Position.Y);
            var moving = (now - previous).sqrMagnitude > StillThreshold;
            previous = now;

            if (moving != wasMoving)
                Switch(moving, instant: false);
        }

        private void Switch(bool moving, bool instant)
        {
            wasMoving = moving;

            var wanted = moving ? walk : idle;
            if (wanted == null)
                return;

            if (instant)
                clips.Play(wanted.name);
            else
                clips.CrossFade(wanted.name, Blend);
        }

        /// <summary>
        /// Aceita tanto o nome cru quanto o prefixado pela armadura. O Blender
        /// exporta os clipes como "CharacterArmature|Walk".
        /// </summary>
        private AnimationState Find(string wanted)
        {
            if (string.IsNullOrWhiteSpace(wanted))
                return null;

            var exact = clips[wanted];
            if (exact != null)
                return exact;

            foreach (AnimationState state in clips)
            {
                var bar = state.name.LastIndexOf('|');
                var tail = bar >= 0 ? state.name.Substring(bar + 1) : state.name;

                if (string.Equals(tail, wanted, StringComparison.OrdinalIgnoreCase))
                    return state;
            }

            var names = new List<string>();
            foreach (AnimationState state in clips)
                names.Add(state.name);

            Debug.LogWarning(
                $"Player: clipe '{wanted}' não encontrado. " +
                $"Disponíveis: {(names.Count == 0 ? "nenhum" : string.Join(", ", names))}", this);
            return null;
        }
    }
}
