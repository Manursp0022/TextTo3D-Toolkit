import torch
from diffusers import FluxPipeline
import sys
# Carica il modello con le ottimizzazioni per ridurre l'uso della VRAM
pipe = FluxPipeline.from_pretrained(
    "black-forest-labs/FLUX.1-schnell", 
    torch_dtype=torch.bfloat16, 
    low_cpu_mem_usage=True  # Modalità per ridurre l'uso di memoria CPU
)

# Abilita l'offloading sequenziale su CPU per ridurre ulteriormente l'uso della GPU
pipe.enable_sequential_cpu_offload()

# Funzione per liberare la cache della GPU
torch.cuda.empty_cache()

prompt = sys.argv[1]

# Disabilita il calcolo dei gradienti e riduci il numero di passi di inferenza
with torch.no_grad():
    image = pipe(
        prompt,
        guidance_scale=0.0,  # Mantieni la guida zero, come nel tuo esempio
        num_inference_steps=4,  # Riduci ulteriormente il numero di passi
        max_sequence_length=256,
        generator=torch.Generator("cpu").manual_seed(0)
    ).images[0]

# Salva l'immagine risultante
image.save("/home/simranjitsin3/TextTo3D-Toolkit/output/output_image.png")

# Liberare memoria dopo l'inferenza
torch.cuda.empty_cache()
