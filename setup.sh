#!/bin/bash

# Load conda environment variables
source /opt/conda/etc/profile.d/conda.sh

# Update system packages
sudo apt update && sudo apt install -y nginx xvfb

# Install Python dependencies
pip install flask
pip install flask_cors

# Generate Nginx configuration
NGINX_CONF="/etc/nginx/sites-available/default"
sudo tee $NGINX_CONF > /dev/null <<EOL
server {
    listen 80 default_server;
    listen [::]:80 default_server;

    root /var/www/html;
    index index.html index.htm index.nginx-debian.html;

    server_name _;

    location / {
        proxy_pass http://127.0.0.1:5000;
        proxy_set_header Host \$host;
        proxy_set_header X-Real-IP \$remote_addr;
        proxy_set_header X-Forwarded-For \$proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto \$scheme;
 	
	proxy_connect_timeout 600s;
        proxy_send_timeout 600s;
        proxy_read_timeout 600s;
        send_timeout 600s;
    }

    location /objects/ {
    	alias /home/thisforbusiness00/TextTo3D-Toolkit/stablefast/output/0;
    }

}
EOL

# Restart Nginx to apply settings
sudo systemctl restart nginx

# Create conda environments if they do not exist
if ! conda info --envs | grep -q 'brou'; then
    conda env create -f envs/newOne.yml
else
    echo "L'ambiente env1B esiste già, saltando la creazione."
fi

if ! conda info --envs | grep -q 'env2New'; then
    conda env create -f envs/env2New.yml
else
    echo "L'ambiente env2New esiste già, saltando la creazione."
fi

conda activate env1B
pip install diffusers
pip install transformers
pip install torch
pip install sentencepiece
pip install accelerate
pip install protobuf
pip install git+https://github.com/huggingface/diffusers.git
conda deactivate

conda activate stable-fast-3d
pip install rembg[gpu]==2.0.57
pip install torch
pip install einops==0.7.0
pip install jaxtyping==0.2.31
pip install omegaconf==2.3.0
pip install transformers==4.42.3
pip install slangtorch==1.2.2
pip install open_clip_torch==2.24.0
pip install trimesh==4.4.1
pip install numpy==1.26.4
pip install huggingface-hub==0.23.4
pip install rembg[gpu]==2.0.57
pip install git+https://github.com/vork/PyNanoInstantMeshes.git
pip install gpytoolbox==0.2.0
huggingface-cli login
conda deactivate

# Start the Flask server
python3 server.py
 
