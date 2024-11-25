# TextTo3D-Toolkit
https://github.com/user-attachments/assets/4fdc894a-bc8b-48c9-823b-338c8c31a673

## How to use TextTo3DPlugin
(It is recommended to run the setup.sh and consequently the Server on a remote server as the models and dependencies will be downloaded on the machine)
# **TextTo3D-Toolkit**

Welcome to the **TextTo3D-Toolkit**, a cutting-edge tool for generating 3D models from text descriptions. The system integrates seamlessly with Unity and is designed to leverage powerful GPU resources for optimal performance. This toolkit is ideal for users who need to rapidly create 3D assets without traditional modeling skills.

---

## **Important Note**
We strongly recommend reading this README **completely** before starting to use the system. Proper configuration and understanding of the instructions are crucial for a successful setup and use.

---

## **Installation**

### **Prerequisites**
Before proceeding, ensure the following are installed on the server or machine:
- **CUDA**: Version 12.3 or later.
- **Conda**: The latest version.
- **Unity**: A compatible version with support for custom editor windows and HTTP services.

---

## **Running on a Remote Server (Recommended)**

### **Firewall Configuration**
If you choose to run the system on a cloud service like **AWS** or **GCP**, you must configure an external IP address and firewall rules to allow traffic to the server:

1. Navigate to **VPC Network** in your cloud management console.
2. Go to **VPC Network > Firewall Rules**.
3. Create a new firewall rule:
   - **Name**: `allow-http`.
   - **Targets**: Select `All instances in the network`.
   - **Source IP ranges**: Enter `0.0.0.0/0` to allow traffic from any IP.
   - **Protocols and ports**: Specify:
     - `tcp:80` for HTTP traffic.
     - `tcp:5000` for the Flask server traffic.
4. Save the rule by clicking **Create**.

---

## **Local Configuration**

1. Update the following files to set the correct paths, IP addresses, and TCP ports:
   - `server.py`
   - `UnityUI/CustomerEditorWindow.cs`

Ensure all placeholders are replaced with your **server's external IP address**, **file paths**, and **TCP ports**.

---

## **Project Setup**

To install the required models and dependencies, run the `setup.sh` script.

`bash setup.sh`

This step may take several minutes, depending on your internet connection and server resources. After that, the server will listen on the port specified in server.py and be ready to handle requests from the Unity interface.
---

## **Unity Interface**

### **Steps to Set Up the Unity Interface**
1. Locate the file `UnityUI/CustomerEditorWindow.cs` in the project directory.
2. **Execute** this file in Unity to enable the custom user interface.
3. Access the UI through the **Custom Window** section in Unity.

### **Entering Prompts**
Within the Unity interface, you can input text prompts to generate 3D models based on descriptions.

### **Unity HTTP Settings**
Ensure that Unity's HTTP service is enabled in the project settings to allow communication with the Flask server.

---

## **Performance Options**

From the Unity interface, you can select between two processing modes:
- **High Efficiency (GPU 24GB)**: Produces a 3D model approximately every **30 seconds**.
- **Optimized Mode (20GB> GPU <24GB)**: Produces a 3D model approximately every **1:40 minutes**.

Choose the mode based on your hardware resources.

---

---

## **Final Notes**
- Ensure all paths, IP addresses, and ports are configured correctly before starting the system.
- The initial setup might require additional time for dependencies to download and install.

For further details, review the source files or reach out to the support team.

---

