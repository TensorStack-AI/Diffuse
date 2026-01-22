## Quantization

Diffuse supports **automatic INT8 quantization** during model load to reduce VRAM usage.

### Supported Backends

Diffuse supports two quantization backends:

1. **quanto**  
   - Used in the default environments  
   - Supports both **CUDA** and **ROCm**

2. **torchao**  
   - Optional CUDA-only environment  
   - Requires a custom environment build

### Key Notes

- Only **INT8 quantization** is currently supported
- Quantization is **automatic** and happens during model loading
- INT8 can reduce VRAM usage by **~30–40%**
- Inference may be **slightly slower** when quantization is enabled

> Quantization is best suited for memory-constrained systems where VRAM is more important than raw speed.