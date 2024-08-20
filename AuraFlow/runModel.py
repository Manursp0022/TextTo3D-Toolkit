from diffusers import AuraFlowPipeline
import torch
import sys

pipeline = AuraFlowPipeline.from_pretrained(
    "fal/AuraFlow-v0.2",
    torch_dtype=torch.float16,
    variant="fp16",
).to("cuda")

image = pipeline(
    prompt=sys.argv[1],
    height=1024,
    width=1024,
    num_inference_steps=50, 
    generator=torch.Generator().manual_seed(666),
    guidance_scale=3.5,
).images[0]

image.save("output/output_image.png")
