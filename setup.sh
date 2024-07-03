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
    	alias /home/thisforbusiness00/TextTo3D-Toolkit/output/0/;
    }

}
EOL

# Restart Nginx to apply settings
sudo systemctl restart nginx

# Create output directory
mkdir -p /home/thisforbusiness00/TextTo3D-Toolkit/output/0

# Create conda environments if they do not exist
if ! conda info --envs | grep -q 'env1'; then
    conda env create -f envs/env1.yml
else
    echo "L'ambiente env1 esiste già, saltando la creazione."
fi

if ! conda info --envs | grep -q 'env2'; then
    conda env create -f envs/env2.yml
else
    echo "L'ambiente env2 esiste già, saltando la creazione."
fi

# Activate env2 and install additional dependencies
conda activate env2
pip install --upgrade setuptools
pip install -r requirements.txt

# Deactivate conda environment
conda deactivate

# Start the Flask server
python3 server.py
