using System.Text.RegularExpressions;

namespace Diffuse
{
    public static class Extensions
    {


    }

    public static class Utils
    {
        public const int FixedIdRange = 1000;

        public static string GetHuggingFaceCacheId(string repositoryUrl)
        {
            return $"models--{repositoryUrl.Replace("/", "--")}";
        }

        public static bool TryParseHuggingFaceRepo(string input, out string repoId)
        {
            repoId = null;

            if (string.IsNullOrWhiteSpace(input))
                return false;

            var match = HuggingFaceRepoRegex.Match(input.Trim());
            if (!match.Success)
                return false;

            repoId = match.Groups["repo"].Value;
            return true;
        }

        private static readonly Regex HuggingFaceRepoRegex = new(@"^(?:https?:\/\/)?(?:www\.)?huggingface\.co\/(?<repo>[^\/\s]+\/[^\/\s]+)$|^(?<repo>[^\/\s]+\/[^\/\s]+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    }
}
