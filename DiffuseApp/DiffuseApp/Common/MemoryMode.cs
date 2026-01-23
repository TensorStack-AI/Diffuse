using System.ComponentModel.DataAnnotations;

namespace Diffuse.Common
{
    public enum MemoryMode
    {
        [Display(Description = "Automatically selects the best memory strategy for the selected device")]
        Auto = 0,

        [Display(Description = "Balances model weights across all available GPUs and the CPU")]
        Balanced = 1,

        [Display(Description = "Sequential CPU offload for minimum GPU memory usage")]
        Lowest = 2,

        [Display(Description = "Model CPU offload with VAE slicing and tiling")]
        Low = 3,

        [Display(Description = "Model CPU Offload")]
        Medium = 4,

        [Display(Description = "All models on the selected device with VAE slicing and tiling")]
        High = 5,

        [Display(Description = "All models on the selected device")]
        Highest = 6
    }
}
