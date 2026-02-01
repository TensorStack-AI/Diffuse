using Diffuse.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using TensorStack.Common;
using TensorStack.Python.Config;

namespace Diffuse
{
    public static class Extensions
    {

        public static int GetIndex(this MemoryProfile profile, int deviceMemory)
        {
            int bestIndex = -1;
            int bestValue = int.MinValue;

            for (int i = 0; i < profile.MemoryModes.Length; i++)
            {
                int value = profile.MemoryModes[i];
                if (value <= deviceMemory && value > bestValue)
                {
                    bestValue = value;
                    bestIndex = i;
                }
            }

            if (bestIndex < 0)
                bestIndex = 0;

            return bestIndex;
        }



        public static bool HasChanged(this IReadOnlyList<LoraAdapterModel> existingAdapters, IReadOnlyList<LoraAdapterModel> newAdapters)
        {
            if (ReferenceEquals(existingAdapters, newAdapters))
                return false;

            if (existingAdapters == null || newAdapters == null)
                return true;

            if (existingAdapters.Count != newAdapters.Count)
                return true;

            for (int i = 0; i < existingAdapters.Count; i++)
            {
                if (!string.Equals(existingAdapters[i]?.Key, newAdapters[i]?.Key, StringComparison.Ordinal))
                    return true;
            }
            return false;
        }



        public static CheckpointConfig ToConfig(this DiffusionCheckpointModel diffusionCheckpoint)
        {
            if (diffusionCheckpoint is null)
                return null;

            if (!string.IsNullOrEmpty(diffusionCheckpoint.Checkpoint))
            {
                return new CheckpointConfig
                {
                    ModelCheckpoint = diffusionCheckpoint.Checkpoint,
                    TextEncoderCheckpoint = diffusionCheckpoint.Checkpoint,
                    VaeCheckpoint = diffusionCheckpoint.Checkpoint
                };
            }

            return new CheckpointConfig
            {
                ModelCheckpoint = diffusionCheckpoint.ModelCheckpoint,
                TextEncoderCheckpoint = diffusionCheckpoint.TextEncoderCheckpoint,
                VaeCheckpoint = diffusionCheckpoint.VaeCheckpoint
            };
        }

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
