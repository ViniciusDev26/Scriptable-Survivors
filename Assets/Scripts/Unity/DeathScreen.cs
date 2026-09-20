using ScriptableSurvivors.Domain;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// O fim da run. Aparece quando o domínio dá o jogador por morto e mostra
    /// o que a partida rendeu.
    ///
    /// Recomeçar recarrega a cena, o que faz o Bootstrap.Awake rodar do zero —
    /// arena, arsenal e upgrades voltam ao estado inicial sem nenhum código de
    /// limpeza. Ter isso na apresentação evita parar e dar Play de novo na
    /// frente da plateia.
    /// </summary>
    public sealed class DeathScreen : MonoBehaviour
    {
        private Arena arena;
        private GameObject panel;
        private Text summary;

        public void Bind(Arena boundArena, Font font, Transform canvas)
        {
            arena = boundArena;
            BuildPanel(font, canvas);
        }

        private void Update()
        {
            if (arena == null || panel == null)
                return;

            if (!arena.IsOver)
                return;

            if (!panel.activeSelf)
                Show();

            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        private void Show()
        {
            var minutes = Mathf.FloorToInt(arena.Elapsed / 60f);
            var seconds = Mathf.FloorToInt(arena.Elapsed % 60f);

            var waves = arena.Waves == null ? string.Empty : $"ONDAS    {arena.Waves.Number}\n";

            summary.text =
                waves +
                $"TEMPO    {minutes}:{seconds:00}\n" +
                $"ABATES   {arena.Kills}\n" +
                $"NÍVEL    {arena.Xp.Level}";

            panel.SetActive(true);
        }

        private void BuildPanel(Font font, Transform canvas)
        {
            var backdrop = UiBuilder.Panel("DeathPanel", canvas, new Color(0.06f, 0.02f, 0.03f, 0.92f));
            UiBuilder.Stretch(backdrop);
            panel = backdrop.gameObject;

            var heading = UiBuilder.Label(
                "Heading", backdrop, font, 72, TextAnchor.MiddleCenter, new Color(0.88f, 0.32f, 0.34f));
            UiBuilder.Place(heading.rectTransform, new Vector2(0f, 200f), new Vector2(1200f, 110f));
            heading.text = "FIM DA RUN";

            summary = UiBuilder.Label(
                "Summary", backdrop, font, 44, TextAnchor.MiddleCenter, Color.white);
            UiBuilder.Place(summary.rectTransform, new Vector2(0f, -20f), new Vector2(900f, 260f));
            summary.lineSpacing = 1.4f;

            var hint = UiBuilder.Label(
                "Hint", backdrop, font, 30, TextAnchor.MiddleCenter, new Color(0.62f, 0.64f, 0.70f));
            UiBuilder.Place(hint.rectTransform, new Vector2(0f, -250f), new Vector2(1200f, 60f));
            hint.text = "pressione  R  para recomeçar";

            panel.SetActive(false);
        }
    }
}
