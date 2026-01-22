## Memory Modes

Memory modes control **how models are placed across GPUs and CPU memory** during inference. They are designed to simplify setup while offering fine-grained control when needed.

| Mode | Description |
|------|-------------|
| **Auto** | Automatically selects the best memory strategy for the selected device(s). |
| **Balanced** | Distributes model weights across all available GPUs and the CPU (multi-GPU setups). |
| **Lowest** | Sequential CPU offload for minimum GPU memory usage (slowest, lowest VRAM). |
| **Low** | Model CPU offload with VAE slicing and tiling enabled. |
| **Medium** | Model CPU offload without VAE slicing or tiling. |
| **High** | All models loaded on the selected device, with VAE slicing and tiling enabled. |
| **Highest** | All models fully loaded on the selected device (fastest, highest VRAM usage). |

> **Tip:**  
> If you’re unsure which mode to use, start with **Auto** — it handles most cases well.