<p align="center" width="100%">
    <img width="25%" src="Assets/Diffuse-Logo-512.png">
</p> 

Diffuse is a Windows desktop UI for Huggingface [Diffusers](https://github.com/huggingface/diffusers). It integrates directly with Python using the Python C API via [CSnakes](https://github.com/tonybaloney/CSnakes), enabling high-performance interop between .NET and Python for running diffusion models.


[![GitHub Release](https://img.shields.io/github/v/release/TensorStack-AI/Diffuse?label=version&color=%2344cc11)](https://github.com/TensorStack-AI/Diffuse/releases)
[![GitHub all releases](https://img.shields.io/github/downloads/TensorStack-AI/Diffuse/total)](https://github.com/TensorStack-AI/Diffuse/releases)
[![GitHub last commit](https://img.shields.io/github/last-commit/TensorStack-AI/Diffuse)](https://github.com/TensorStack-AI/Diffuse/commits/master/)
[![Discord](https://img.shields.io/discord/1457477275246268451?label=Discord&)](https://discord.gg/ptgMMv36Xu)
---

## Features
- Automatic installation of isolated portable Python
- Device-specific Python virtual environments
- Automatic model downloads from Huggingface repositories

---


## Supported Pipelines
- **Z-Image:** ZImagePipeline, ZImageImg2ImgPipeline
- **Qwen Image:** QwenImagePipeline, QwenImageImg2ImgPipeline, QwenImageEditPlusPipeline
- **FLUX.1:** FluxPipeline, FluxImg2ImgPipeline, FluxKontextPipeline, FluxControlNetPipeline
- **FLUX.2:** Flux2Pipeline
- **Chroma:** ChromaPipeline, ChromaImg2ImgPipeline
- **LTX-Video:** LTXPipeline, LTXImageToVideoPipeline
- **Wan Video:** WanPipeline, WanImageToVideoPipeline
- **CogVideoX:** CogVideoXPipeline, CogVideoXImageToVideoPipeline, CogVideoXVideoToVideoPipeline
- **Kandinsky5:** Kandinsky5T2IPipeline, Kandinsky5I2IPipeline, Kandinsky5T2VPipeline, Kandinsky5I2VPipeline
- **StableDiffusionXL:** StableDiffusionXLPipeline, StableDiffusionXLImg2ImgPipeline, StableDiffusionXLControlNetPipeline, StableDiffusionXLControlNetImg2ImgPipeline

---

## Installation

### 1. Installer Version
1. Download and run **Diffuse_vX.X.X.exe**  
2. Follow the on-screen instructions

### 2. Standalone Version
1. Download and extract **Diffuse_vX.X.X.zip**  
   *A fast SSD with plenty of free space is recommended, as model downloads can be large.*

2. Run **Diffuse.exe**

Diffuse will automatically:
   - Install an isolated portable Python runtime  
   - Create the required virtual environment  
   - Download the selected model from Hugging Face  

### First-run notice
On first launch or when loading a model for the first time, setup may take several minutes while Python, dependencies, and model files are downloaded and initialized. This is expected behavior.


No manual Python setup is required.

### Device Support
Supports CUDA and ROCM  based devices


---

## Project Roadmap

### Alpha
Proof of concept, Focus on core functionality.
- ~~Portable Python installation and management~~
- ~~Device-specific virtual environments~~
- ~~Minimal but functional Windows UI~~
- ~~Basic Diffusers pipeline support~~

### Beta
Focus on usability, stability, and feature expansion.
- ~~Fully isolated Python execution~~
- ~~Multiple virtual environments~~
- ~~Installer and deployment tooling~~
- ~~Upscaling and interpolation support~~
- ~~Extractor pipeline support~~
- ~~ControlNet support~~
- ~~Inpaint/Outpaint processes~~
- Advanced UI and workflow options
- GGUF model support
- Weighted prompt support
- Model Manager, download queuing, online templates
- Stability, performance, and reliability improvements

---

## Screenshots

### TextToImage
<p align="center" width="100%">
    <img src="Assets/Screenshots/TextToImage.png">
</p> 

### ImageToImage
<p align="center" width="100%">
    <img src="Assets/Screenshots/ImageToImage.png">
</p> 

### ImageEdit
<p align="center" width="100%">
    <img src="Assets/Screenshots/ImgeEdit.png">
</p> 

### ImageEdit-Multi
<p align="center" width="100%">
    <img src="Assets/Screenshots/ImgeEdit-Multi.png">
</p> 

### TextToVideo
<p align="center" width="100%">
    <img src="Assets/Screenshots/TextToVideo.png">
</p> 

### ImageToVideo
<p align="center" width="100%">
    <img src="Assets/Screenshots/ImageToVideo.png">
</p> 