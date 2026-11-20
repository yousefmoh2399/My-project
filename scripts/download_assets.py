import os
import ssl
import urllib.request
import trimesh

ctx = ssl.create_default_context()
ctx.check_hostname = False
ctx.verify_mode = ssl.CERT_NONE

models = {
    # Units
    "Assets/KINGWorldWar/Art/Models/Units/Tank_Medium.obj": "https://polyfork.dev/cdn/medium-tank-65da6b.glb",
    "Assets/KINGWorldWar/Art/Models/Units/Armored_Vehicle.obj": "https://polyfork.dev/cdn/armoured-half-track-3deded.glb",
    "Assets/KINGWorldWar/Art/Models/Units/Artillery.obj": "https://polyfork.dev/cdn/anti-aircraft-gun-93cc80.glb",
    "Assets/KINGWorldWar/Art/Models/Units/Fighter_Jet.obj": "https://polyfork.dev/cdn/single-engine-fighter-a6addf.glb",
    "Assets/KINGWorldWar/Art/Models/Units/Recon_Plane.obj": "https://polyfork.dev/cdn/twin-engine-piston-light-f417c7.glb",
    
    # Buildings
    "Assets/KINGWorldWar/Art/Models/Buildings/HQ_Tower.obj": "https://polyfork.dev/cdn/control-tower-c95a65.glb",
    "Assets/KINGWorldWar/Art/Models/Buildings/Radar_Station.obj": "https://polyfork.dev/cdn/observation-tower-69b692.glb",
    "Assets/KINGWorldWar/Art/Models/Buildings/Supply_Depot.obj": "https://polyfork.dev/cdn/fuel-depot-e57cdc.glb",
    "Assets/KINGWorldWar/Art/Models/Buildings/Vehicle_Factory.obj": "https://polyfork.dev/cdn/maintenance-hangar-6fab99.glb",
    "Assets/KINGWorldWar/Art/Models/Buildings/Barracks.obj": "https://polyfork.dev/cdn/brick-depot-warehouse-3dd642.glb",
}

for dest_path, url in models.items():
    os.makedirs(os.path.dirname(dest_path), exist_ok=True)
    temp_glb = f"/tmp/{os.path.basename(dest_path)}.glb"
    
    print(f"Downloading {url}...")
    req = urllib.request.Request(url, headers={'User-Agent': 'Mozilla/5.0'})
    with urllib.request.urlopen(req, context=ctx) as resp, open(temp_glb, 'wb') as f:
        f.write(resp.read())
        
    print(f"Converting to {dest_path}...")
    scene = trimesh.load(temp_glb)
    obj_data = trimesh.exchange.obj.export_obj(scene)
    with open(dest_path, 'w') as f:
        f.write(obj_data)
        
    print(f"Saved {dest_path} ({os.path.getsize(dest_path)} bytes)")

print("ALL ASSETS DOWNLOADED AND CONVERTED SUCCESSFULLY!")
