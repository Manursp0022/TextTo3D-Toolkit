from flask import Flask, request, jsonify, send_from_directory
import subprocess
import os
import shutil
import threading
import time
import traceback
from flask_cors import CORS
app = Flask(__name__)
CORS(app)
last_request_time = time.time()
timeout = 600  # Timeout in secondi (600 secondi = 10 minuti)
 # Variabile globale per memorizzare l'URL dell'oggetto 3D

def monitor_timeout():
    global last_request_time
    while True:
        if time.time() - last_request_time > timeout:
            print("Nessuna richiesta ricevuta. Spegnimento del server Flask.")
            os._exit(0) 
        time.sleep(60) 

@app.route('/')
def home():
    return "Server Flask Funzionante!"

@app.route('/process', methods=['POST'])
def process_description():
    global last_request_time
    last_request_time = time.time() 

    description = request.form['description']
    
    # Path to Image and object source
    image_path = "/home/thisforbusiness00/TextTo3D-Toolkit/output/output_image.png"
    model_output_dir = "/home/thisforbusiness00/TextTo3D-Toolkit/output/"
    model_output_path = os.path.join(model_output_dir, "3DModel.obj")
    
    try:
        print("Ricevuta descrizione:", description)

        # First Model(Text to Image)
        print("Eseguendo il primo modello...")
        result1 = subprocess.run(f"/bin/bash -c 'source /opt/conda/etc/profile.d/conda.sh && conda activate env1 && python sdxl-flash/runModel.py \"{description}\"'", shell=True, check=True)
        print(f"Primo modello eseguito con successo: {result1}")

        # Check if the Image has been created 
        if not os.path.isfile(image_path):
            print(f"Immagine non creata: {image_path}")
            return jsonify({"error": "Immagine non creata"}), 500
        print(f"Immagine creata: {image_path}")

        # Second Model(Image to 3d Model)
        print("Eseguendo il secondo modello...")
        result2 = subprocess.run(f"/bin/bash -c 'source /opt/conda/etc/profile.d/conda.sh && conda deactivate && conda activate env2 && python Tripo/run.py \"{image_path}\" --bake-texture --texture-resolution 4096 --output-dir \"{model_output_dir}\"'", shell=True, check=True)
        print(f"Secondo modello eseguito con successo: {result2}")

        # Building the object and texture url
        object_url = f"http://34.65.36.39:5000/objects/3DModel.obj"
        texture_url = f"http://34.65.36.39:5000/objects/texture.png"
        print(f"URL del modello 3D: {object_url}")
        print(f"URL della texture: {texture_url}")


        # Sending the response to Unity
        response = jsonify({"object_url": object_url, "texture_url": texture_url})
        response.headers.add("Content-Type", "application/json")
        return response

        print(f"Risposta JSON inviata: {response.get_data(as_text=True)}")

    except subprocess.CalledProcessError as e:
        print("CalledProcessError:", e)
        print(traceback.format_exc())
        return jsonify({"error": str(e)}), 500
    except Exception as e:
        print("Exception:", e)
        print(traceback.format_exc())
        return jsonify({"error": str(e)}), 500

@app.route('/objects/<path:filename>')
def serve_object(filename):
    return send_from_directory('/home/thisforbusiness00/TextTo3D-Toolkit/output/0/', filename)

if __name__ == '__main__':
    timeout_thread = threading.Thread(target=monitor_timeout)
    timeout_thread.daemon = True 
    timeout_thread.start()
    app.run(host='0.0.0.0', port=5000)
