import os
import json
import zipfile

os.system("dotnet build CustomInteriorRate.sln --configuration Release")

try:
    os.makedirs("./dist")
except:
    pass

manifest = json.load(open("./manifest.json"))

with zipfile.ZipFile(f"./dist/CustomInteriorRate_v{manifest['version_number']}.zip", "w") as zipf:
    zipf.write("./bin/Release/netstandard2.1/com.eigeen.lethal.CustomInteriorRate.dll")
    zipf.write("./README.md")
    zipf.write("./manifest.json")
    zipf.write("./icon.png")

