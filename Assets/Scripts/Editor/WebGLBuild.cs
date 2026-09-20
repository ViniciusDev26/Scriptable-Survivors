using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace ScriptableSurvivors.Build
{
    /// <summary>
    /// Build WebGL por linha de comando, para a entrega ser um comando
    /// repetível em vez de uma sequência de cliques que alguém precisa lembrar.
    ///
    ///   Unity -batchmode -nographics -projectPath . -buildTarget WebGL \
    ///         -executeMethod ScriptableSurvivors.Build.WebGLBuild.Build \
    ///         -logFile -
    /// </summary>
    public static class WebGLBuild
    {
        private const string OutputPath = "Build/WebGL";

        [MenuItem("Game/Build WebGL")]
        public static void Build()
        {
            var scenes = EditorBuildSettings.scenes
                .Where(scene => scene.enabled)
                .Select(scene => scene.path)
                .ToArray();

            if (scenes.Length == 0)
            {
                Finish(false, "Nenhuma cena habilitada em Build Settings.");
                return;
            }

            // Trocar de plataforma recarrega os assemblies, o que pode abortar
            // este método no meio. Por isso a troca é um passo separado: a
            // primeira execução troca, a segunda constrói.
            //
            // Em batchmode isso nunca dispara, porque -buildTarget WebGL já
            // resolve a plataforma antes do -executeMethod.
            if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            {
                Debug.Log(
                    "[build] plataforma ativa nao e WebGL. Trocando agora — isso reimporta " +
                    "todos os assets e demora. Quando terminar, rode Game > Build WebGL de novo.");

                EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
                return;
            }

            // Brotli comprime o .wasm em torno de 4 a 5 vezes. O navegador só
            // descomprime nativamente se o servidor mandar Content-Encoding: br
            // — e quando não manda, o jogo não abre, com uma mensagem inútil.
            //
            // O fallback embute um descompressor em JavaScript: com o cabeçalho
            // certo o navegador faz nativo e rápido; sem ele o JavaScript
            // assume. Isso faz a build abrir em qualquer servidor estático,
            // inclusive um python -m http.server, ao custo de um carregador
            // maior e de uma descompressão mais lenta no caso ruim.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
            PlayerSettings.WebGL.decompressionFallback = true;

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = OutputPath,
                target = BuildTarget.WebGL,
                targetGroup = BuildTargetGroup.WebGL,
                options = BuildOptions.None,
            };

            var summary = BuildPipeline.BuildPlayer(options).summary;

            Debug.Log(
                $"[build] resultado={summary.result} " +
                $"tamanho={summary.totalSize / 1048576} MB " +
                $"duracao={summary.totalTime} " +
                $"erros={summary.totalErrors}");

            if (summary.result == BuildResult.Succeeded)
            {
                Debug.Log(
                    $"[build] pronto em {OutputPath}. Para abrir no navegador, sirva a pasta " +
                    "por HTTP — abrir o index.html direto do disco nao funciona.");
            }

            Finish(summary.result == BuildResult.Succeeded, summary.result.ToString());
        }

        private static void Finish(bool succeeded, string message)
        {
            if (!succeeded)
                Debug.LogError($"[build] falhou: {message}");

            if (Application.isBatchMode)
                EditorApplication.Exit(succeeded ? 0 : 1);
        }
    }
}
