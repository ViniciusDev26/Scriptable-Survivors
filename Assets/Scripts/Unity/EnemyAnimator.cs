using System;
using System.Collections.Generic;
using ScriptableSurvivors.Domain.Enemies;
using UnityEngine;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// Toca as animações do inimigo: anda em loop e se contorce ao levar dano.
    ///
    /// Escuta o evento Damaged do domínio — o mesmo desenho de ProjectileFired
    /// e EnemySpawned. O corpo reage ao que a simulação decidiu; ele não
    /// descobre o golpe comparando vida entre quadros.
    /// </summary>
    public sealed class EnemyAnimator : MonoBehaviour
    {
        private const float Blend = 0.06f;

        private Enemy enemy;
        private Animation clips;
        private AnimationState walk;
        private AnimationState hit;

        public void Bind(Enemy boundEnemy, Animation animation, string walkClipName, string hitClipName)
        {
            enemy = boundEnemy;
            clips = animation;

            walk = Find(walkClipName);
            hit = Find(hitClipName);

            if (walk != null)
            {
                // O wrapMode vai no ESTADO: o do componente só vale como padrão
                // para estados criados depois, e os do importador já existem.
                walk.wrapMode = WrapMode.Loop;
                clips.Play(walk.name);
            }

            if (hit != null)
                hit.wrapMode = WrapMode.Once;

            if (hit != null && walk != null)
                enemy.Health.Damaged += OnDamaged;
        }

        private void OnDestroy()
        {
            if (enemy != null)
                enemy.Health.Damaged -= OnDamaged;
        }

        private void OnDamaged(float amount)
        {
            if (enemy.Health.IsDead)
                return;

            // Sob fogo de três tiros por segundo, reiniciar a contorção a cada
            // acerto deixaria o inimigo em espasmo permanente, sem nunca voltar
            // a andar. O primeiro golpe manda; os de dentro da janela não.
            if (clips.IsPlaying(hit.name))
                return;

            clips.CrossFade(hit.name, Blend);
            clips.CrossFadeQueued(walk.name, Blend, QueueMode.CompleteOthers);
        }

        /// <summary>
        /// Aceita tanto o nome cru quanto o prefixado pela armadura. O Blender
        /// exporta os clipes como "CharacterArmature|Walk", e exigir isso no
        /// Inspector seria uma pegadinha esperando o próximo modelo.
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

            Debug.LogWarning(
                $"{name}: clipe '{wanted}' não encontrado. Disponíveis: {Available()}", this);
            return null;
        }

        private string Available()
        {
            var names = new List<string>();
            foreach (AnimationState state in clips)
                names.Add(state.name);

            return names.Count == 0 ? "nenhum" : string.Join(", ", names);
        }
    }
}
