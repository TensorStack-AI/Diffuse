## Diffuse Environments

Diffuse manages isolated **Python virtual environments** to ensure pipelines run with the correct dependencies for each hardware vendor, device, and pipeline type.

### Why Environments Exist

Different GPUs, drivers, and pipelines often require **different Python packages and versions**.  
Trying to run everything in one shared Python environment quickly leads to conflicts, crashes, or broken installs.

Diffuse solves this by giving each setup its **own isolated environment**, so:

- NVIDIA and AMD dependencies never clash
- Pipelines can use exactly the versions they need
- Updating or rebuilding one environment won’t break others
- Experiments stay contained and predictable

In short: **environments keep Diffuse stable, reproducible, and easy to recover when something goes wrong**.

---

### Overview

- Environments are **Python `venv`-based**
- Each environment runs in an **external process**
- Communication with Diffuse uses **named-pipe IPC**
- Environments may define **custom environment variables**

---

### Lifecycle

- An environment is **automatically launched when a pipeline is loaded**
- The environment **shuts down when the pipeline is unloaded**
- Environments can be **created, rebuilt, or deleted on demand**

---

### Vendor & Device Isolation

All environments are **vendor-specific** (for example: NVIDIA or AMD).

More granular environments can also be defined:

- **Device-specific environments**  
  _Example:_ `Vendor=AMD, Device=W7900`

- **Pipeline-specific environments**  
  _Example:_ `Vendor=AMD, Pipeline=FluxPipeline`

This allows Diffuse to safely handle incompatible runtimes, drivers, and Python dependencies without conflict.

---

### Environment Selection & Precedence

When loading a pipeline, Diffuse selects the environment using the following precedence order:

1. **Pipeline-specific environment**
2. **Device-specific environment**
3. **Vendor-specific environment**

The most specific matching environment is always chosen.
