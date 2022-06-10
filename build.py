import shutil
import subprocess
import time

import psutil


start_time = time.time()

p = subprocess.Popen("dotnet build", stdout=subprocess.PIPE, stderr=subprocess.PIPE, shell=True)
stdout, stderr = p.communicate()
if p.returncode != 0:
    print(stdout.decode())
    exit(p.returncode)

print(f"built in {time.time() - start_time:.2f}s")

for proc in psutil.process_iter(["name", "pid"]):
    if proc.name() == "Stacklands.exe":
        proc.kill()
        proc.wait()

dll = "./bin/Debug/netstandard2.0/BerryLoader.dll"
shutil.copyfile(dll, "../../Game/BepInEx/plugins/BerryLoader/BerryLoader.dll")
shutil.copyfile(dll, "../ModTest/BerryLoader.dll")

subprocess.Popen("..\..\Game\Stacklands")
