using ScriptableSurvivors.Domain;
using ScriptableSurvivors.Domain.Progression;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = System.Random;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// A tela de três cartas. Aparece quando o domínio diz que há um nível
    /// pendente e some quando a carta é escolhida — ela não decide nada, só
    /// mostra o que a Arena já resolveu.
    ///
    /// A escolha é por teclado (1, 2, 3) de propósito. Botão clicável exigiria
    /// EventSystem, GraphicRaycaster e um módulo de input com action maps — três
    /// peças a mais para falhar em WebGL. E, ao vivo, apertar uma tecla é mais
    /// rápido e mais seguro do que mirar o mouse na frente da plateia.
    /// </summary>
    public sealed class UpgradeScreen : MonoBehaviour
    {
        private const int CardCount = 3;

        private readonly Dictionary<string, UpgradeData> catalog = new Dictionary<string, UpgradeData>();
        private readonly List<Text> cardLabels = new List<Text>();
        private readonly List<RectTransform> cards = new List<RectTransform>();

        private Arena arena;
        private UpgradePool pool;
        private Random random;
        private GameObject panel;
        private IReadOnlyList<Upgrade> offered;

        public void Bind(
            Arena boundArena,
            UpgradePool upgradePool,
            Random rng,
            IEnumerable<UpgradeData> sources,
            Font font,
            Transform canvas)
        {
            arena = boundArena;
            pool = upgradePool;
            random = rng;

            foreach (var data in sources)
            {
                if (data != null)
                    catalog[data.name] = data;
            }

            BuildPanel(font, canvas);
        }

        private void Update()
        {
            if (arena == null || panel == null)
                return;

            if (!arena.IsAwaitingUpgrade)
            {
                if (panel.activeSelf)
                    panel.SetActive(false);
                return;
            }

            if (!panel.activeSelf)
                Offer();

            var picked = ReadChoice();
            if (picked < 0 || picked >= offered.Count)
                return;

            arena.Choose(offered[picked]);
            panel.SetActive(false);
        }

        private void Offer()
        {
            offered = pool.Draw(CardCount, random);

            for (var i = 0; i < cards.Count; i++)
            {
                var visible = i < offered.Count;
                cards[i].gameObject.SetActive(visible);

                if (!visible)
                    continue;

                catalog.TryGetValue(offered[i].Id, out var data);
                var title = data != null ? data.Title : offered[i].Id;
                var description = data != null ? data.Description : string.Empty;

                cardLabels[i].text = $"<b>{i + 1}</b>\n\n{title}\n\n{description}";
            }

            panel.SetActive(true);
        }

        private static int ReadChoice()
        {
            var keyboard = Keyboard.current;
            if (keyboard == null)
                return -1;

            if (keyboard.digit1Key.wasPressedThisFrame) return 0;
            if (keyboard.digit2Key.wasPressedThisFrame) return 1;
            if (keyboard.digit3Key.wasPressedThisFrame) return 2;
            return -1;
        }

        private void BuildPanel(Font font, Transform canvas)
        {
            var backdrop = UiBuilder.Panel("UpgradePanel", canvas, new Color(0.04f, 0.05f, 0.08f, 0.88f));
            UiBuilder.Stretch(backdrop);
            panel = backdrop.gameObject;

            var heading = UiBuilder.Label(
                "Heading", backdrop, font, 56, TextAnchor.MiddleCenter, Color.white);
            UiBuilder.Place(heading.rectTransform, new Vector2(0f, 260f), new Vector2(1200f, 90f));
            heading.text = "SUBIU DE NÍVEL";

            var hint = UiBuilder.Label(
                "Hint", backdrop, font, 28, TextAnchor.MiddleCenter, new Color(0.65f, 0.68f, 0.74f));
            UiBuilder.Place(hint.rectTransform, new Vector2(0f, -250f), new Vector2(1200f, 60f));
            hint.text = "escolha com  1   2   3";

            const float width = 420f;
            const float gap = 40f;

            for (var i = 0; i < CardCount; i++)
            {
                var card = UiBuilder.Panel($"Card{i}", backdrop, new Color(0.13f, 0.15f, 0.20f, 1f));
                UiBuilder.Place(card, new Vector2((i - 1) * (width + gap), 0f), new Vector2(width, 320f));

                var label = UiBuilder.Label(
                    "Text", card, font, 34, TextAnchor.MiddleCenter, Color.white);
                UiBuilder.Place(label.rectTransform, Vector2.zero, new Vector2(width - 60f, 260f));
                label.supportRichText = true;

                cards.Add(card);
                cardLabels.Add(label);
            }

            panel.SetActive(false);
        }
    }
}
