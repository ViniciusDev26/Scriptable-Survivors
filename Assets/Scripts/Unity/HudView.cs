using ScriptableSurvivors.Domain;
using UnityEngine;
using UnityEngine.UI;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// Mostra abates e vida. Lê o domínio e escreve texto — não guarda
    /// contagem própria, para não existir um segundo número que possa
    /// divergir do verdadeiro.
    /// </summary>
    public sealed class HudView : MonoBehaviour
    {
        private Arena arena;
        private Text label;

        private int lastKills = -1;
        private int lastHealth = -1;

        public void Bind(Arena boundArena, Text boundLabel)
        {
            arena = boundArena;
            label = boundLabel;
            Refresh();
        }

        private void LateUpdate()
        {
            if (arena != null)
                Refresh();
        }

        private void Refresh()
        {
            var kills = arena.Kills;
            var health = Mathf.CeilToInt(arena.Player.Health.Current);

            // Remontar a string todo quadro geraria lixo para o coletor à toa,
            // e em WebGL isso aparece como engasgo.
            if (kills == lastKills && health == lastHealth)
                return;

            lastKills = kills;
            lastHealth = health;

            label.text = arena.IsOver
                ? $"ABATES  {kills}\n\nFIM DA RUN"
                : $"ABATES  {kills}\nVIDA    {health}";
        }
    }
}
