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
    use_less_than_15GB_str = request.form.get('use_less_than_15GB', 'False')
    use_less_than_15GB = use_less_than_15GB == 'True'
    
    # Path to Image and object source
    image_path = "/home/simranjitsin3/TextTo3D-Toolkit/output/output_image.png"
    model_output_dir = "/home/simranjitsin3/TextTo3D-Toolkit/stablefast3D/output/"
    model_output_path = os.path.join(model_output_dir, "mesh.glb")
    
    try:
        print("Ricevuta descrizione:", description)
	print("Usa meno di 15GB:", use_less_than_15GB)

           if use_less_than_15GB:
	    print("Eseguendo il primo modello, con meno di 24GB")
            result1 = subprocess.run(
               f"/bin/bash -c 'source /opt/conda/etc/profile.d/conda.sh && conda activate env1 && python FluxModel/runFlux.py \"Create a 3D high-Quality Render {description}\"'", shell=True, check=True)
            )
        else:
	    print("Eseguendo il primo modello, con più di 24GB")
            result1 = subprocess.run(
                f"/bin/bash -c 'source /opt/conda/etc/profile.d/conda.sh && conda activate env1 && python FluxModel/runFluxOF.py \"Create a 3D high-Quality Render {description}\"'", shell=True, check=True)
            )
	print(f"Primo modello eseguito con successo: {result1}")
        

        # Check if the Image has been created 
        if not os.path.isfile(image_path):
            print(f"Immagine non creata: {image_path}")
            return jsonify({"error": "Immagine non creata"}), 500
        print(f"Immagine creata: {image_path}")

        # Second Model(Image to 3d Model)
        print("Eseguendo il secondo modello...")
        result2 = subprocess.run(f"/bin/bash -c 'source /opt/conda/etc/profile.d/conda.sh && conda deactivate && conda activate stable-fast-3d && python stablefast3D/run.py \"{image_path}\" --output-dir \"{model_output_dir}\"'", shell=True, check=True)
        print(f"Secondo modello eseguito con successo: {result2}")

        # Building the object and texture url
        object_url = f"http://34.64.171.38:5000/objects/mesh.glb"
        #texture_url = f"http://34.65.54.36:5000/objects/texture.png"
        print(f"URL del modello 3D: {object_url}")
        #print(f"URL della texture: {texture_url}")


        # Sending the response to Unity
        #response = jsonify({"object_url": object_url, "texture_url": texture_url})
        response = jsonify({"object_url":object_url})
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

#@app.route('/objects/<path:filename>')
#def serve_object(filename):
   #return send_from_directory('/home/thisforbusiness00/TextTo3D-Toolkit/stablefast3D/output/0', filename)
@app.route('/objects/<path:filename>')
def serve_object(filename):
    directory_path = '/home/simranjitsin3/TextTo3D-Toolkit/stablefast3D/output/0'

    # Servire il file
    response = send_from_directory(directory_path, filename)

    # Dopo aver servito il file, eseguire il comando di eliminazione della directory
    def remove_directory():
        try:
            subprocess.run(['rm', '-r', directory_path], check=True)
            print(f"Directory {directory_path} rimossa con successo.")
        except subprocess.CalledProcessError as e:
            print(f"Errore durante la rimozione della directory: {e}")

    # Eseguire la rimozione della directory in background
    remove_directory()

    return response

if __name__ == '__main__':
    timeout_thread = threading.Thread(target=monitor_timeout)
    timeout_thread.daemon = True 
    timeout_thread.start()
    app.run(host='0.0.0.0', port=5000)
