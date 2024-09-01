from diffusers import AuraFlowPipeline
import torch
import sys
# Carica la pipeline
pipeline = AuraFlowPipeline.from_pretrained(
    "fal/AuraFlow-v0.3",
    torch_dtype=torch.float16,
    variant="fp16",
)

# Sposta i moduli su GPU o CPU solo se supportano il metodo `.to()`
if hasattr(pipeline, 'text_encoder'):
    pipeline.text_encoder.to('cuda')  # Sposta text_encoder su GPU

if hasattr(pipeline, 'transformer'):  # Verifica se esiste 'transformer'
    pipeline.transformer.to('cuda')  # Sposta transformer su GPU, se esiste

if hasattr(pipeline, 'vae'):
    pipeline.vae.to('cuda')  # Sposta VAE su GPU per evitare mismatch di tipo

# Genera l'immagine
image = pipeline(
    prompt=sys.argv[1],
    width=1536,  # Risoluzione originale
    height=768,  # Risoluzione originale
    num_inference_steps=50,  # Numero di passi di inferenza
    generator=torch.Generator().manual_seed(1),
    guidance_scale=3.5,
).images[0]

# Salva l'immagine generata
image.save("output.png")
