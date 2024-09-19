import torch
from diffusers import FluxPipeline
import sys
pipe = FluxPipeline.from_pretrained("black-forest-labs/FLUX.1-schnell", torch_dtype=torch.bfloat16)
pipe.enable_model_cpu_offload()
prompt = sys.argv[1]
image = pipe(
    prompt,
    guidance_scale=0.0,
    num_inference_steps=4,
    max_sequence_length=512,
    generator=torch.Generator("cpu")
).images[0]
image.save("PathTo/output_image.png")
