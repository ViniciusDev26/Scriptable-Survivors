using UnityEngine;
using Numerics = System.Numerics;

namespace ScriptableSurvivors.Unity
{
    /// <summary>
    /// Arrasto de dedo como direção. Onde o dedo encosta vira a origem; o
    /// quanto ele se afasta dali vira o vetor. Soltar zera.
    ///
    /// Sem desenho e sem asset: não há stick na tela, nem canto fixo para
    /// acertar. A mão pode estar de qualquer lado do aparelho.
    ///
    /// Devolve o mesmo vetor que o teclado devolveria, então o domínio não
    /// sabe de onde veio o movimento. PlanarInput cuida da rotação da câmera
    /// nos dois casos, e empurrão parcial já era suportado desde o Dia 1.
    /// </summary>
    internal sealed class TouchStick
    {
        /// <summary>
        /// Distância de arrasto para pedir velocidade máxima, como fração do
        /// menor lado da tela. Em pixels fixos, um aparelho de densidade alta
        /// exigiria arrastar o dedo até a borda.
        /// </summary>
        private const float ReachFraction = 0.15f;

        private Vector2 origin;
        private bool active;

        public Numerics.Vector2 Direction { get; private set; }

        /// <summary>
        /// <paramref name="accepting"/> diz se a partida aceita movimento. Com
        /// a tela de cartas ou a de morte abertas, o toque pertence a elas — e
        /// escolher uma carta não pode sair andando com o personagem junto.
        /// </summary>
        public void Read(bool accepting)
        {
            if (!accepting || !PointerInput.Held)
            {
                Release();
                return;
            }

            if (!active)
            {
                active = true;
                origin = PointerInput.Position;
                Direction = Numerics.Vector2.Zero;
                return;
            }

            var offset = PointerInput.Position - origin;
            var distance = offset.magnitude;
            if (distance < 0.001f)
            {
                Direction = Numerics.Vector2.Zero;
                return;
            }

            var reach = Mathf.Min(Screen.width, Screen.height) * ReachFraction;

            // Sem o teto, arrastar mais longe andaria mais rápido — e a
            // velocidade é do domínio, não do dedo.
            var strength = Mathf.Min(distance, reach) / reach;
            var aim = offset / distance;

            Direction = new Numerics.Vector2(aim.x * strength, aim.y * strength);
        }

        private void Release()
        {
            active = false;
            Direction = Numerics.Vector2.Zero;
        }
    }
}
