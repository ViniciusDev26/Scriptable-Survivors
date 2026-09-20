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

            // Sem compressão o resultado abre em qualquer servidor estático.
            // Brotli e Gzip exigem cabeçalhos configurados no servidor, e
            // descobrir isso na véspera da apresentação seria caro.
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;

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
