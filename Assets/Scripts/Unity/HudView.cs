using ScriptableSurvivors.Domain;
using ScriptableSurvivors.Domain.Combat;
using ScriptableSurvivors.Domain.Players;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// Mostra abates, vida, nível e os bônus acumulados. Lê tudo do domínio —
    /// não mantém contagem própria, para não existir um segundo número capaz
    /// de divergir do verdadeiro.
    ///
    /// A lista de bônus só é possível porque a pilha de modificadores guarda o
    /// que os upgrades somaram separado da base vinda do catálogo. Se os
    /// números tivessem sido substituídos, aqui só daria para mostrar o total.
    /// </summary>
    public sealed class HudView : MonoBehaviour
    {
        private readonly StringBuilder text = new StringBuilder();
        private readonly StringBuilder line = new StringBuilder();

        private Arena arena;
        private Text label;

        private int lastKills = -1;
        private int lastHealth = -1;
        private int lastLevel = -1;
        private int lastXp = -1;
        private int lastModifierVersion = -1;

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
            var level = arena.Xp.Level;
            var xp = arena.Xp.Current;
            var modifierVersion = arena.Modifiers.Version;

            // Remontar a string todo quadro geraria lixo para o coletor à toa,
            // e em WebGL isso aparece como engasgo.
            if (kills == lastKills
                && health == lastHealth
                && level == lastLevel
                && xp == lastXp
                && modifierVersion == lastModifierVersion)
                return;

            lastKills = kills;
            lastHealth = health;
            lastLevel = level;
            lastXp = xp;
            lastModifierVersion = modifierVersion;

            text.Clear();
            text.Append("ABATES  ").Append(kills).Append('\n');
            text.Append("VIDA    ").Append(health)
                .Append(" / ").Append(Mathf.CeilToInt(arena.Player.Health.Max)).Append('\n');
            text.Append("NÍVEL   ").Append(level)
                .Append("   (").Append(xp).Append('/').Append(arena.Xp.Required).Append(')');

            AppendBonuses();

            label.text = text.ToString();
        }

        private void AppendBonuses()
        {
            var active = arena.Modifiers.ActiveStats;
            if (active.Count == 0)
                return;

            text.Append("\n\nBÔNUS");

            for (var i = 0; i < active.Count; i++)
            {
                text.Append("\n  ");
                text.Append(StatLabels.Describe(arena.Modifiers, active[i], line));
            }
        }
    }
}
