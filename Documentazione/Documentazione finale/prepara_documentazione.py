"""Verifica sorgenti/PNG senza riscrivere il documento unico del gruppo."""
from pathlib import Path
import argparse
import hashlib
import json
import re
import subprocess
import sys
sys.stdout.reconfigure(encoding="utf-8")

ROOT = Path(__file__).resolve().parent
DOC = ROOT.parent

def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()

def prepare():
    (ROOT / "_build").mkdir(exist_ok=True)

def verify_diagrams():
    from PIL import Image
    records = []
    for source in sorted(DOC.rglob("*.puml")):
        if "_build" in source.parts:
            continue
        text = source.read_text(encoding="utf-8")
        blocks = re.findall(r"(?ms)^@startuml[^\n]*\n.*?^@enduml[^\n]*", text)
        if not blocks:
            raise RuntimeError(f"Nessun diagramma in {source}")
        for block in blocks:
            header = block.splitlines()[0].split(maxsplit=1)
            name = header[1].strip() if len(header) > 1 else source.stem
            image = source.with_name(name + ".png")
            if not image.is_file():
                raise FileNotFoundError(image)
            with Image.open(image) as im:
                metadata = im.info.get("plantuml", "")
                embedded_blocks = re.findall(r"(?ms)^@startuml[^\n]*\n.*?^@enduml[^\n]*", metadata)
                embedded = embedded_blocks[0] if len(embedded_blocks) == 1 else ""
                normalize = lambda s: s.replace("\r\n", "\n").strip()
                if normalize(embedded) != normalize(block):
                    raise RuntimeError(f"Sorgente e PNG non coerenti: {image}")
                size = list(im.size)
            records.append({
                "source": source.relative_to(DOC).as_posix(),
                "image": image.relative_to(DOC).as_posix(),
                "source_sha256": sha(source), "image_sha256": sha(image),
                "pixel_size": size, "embedded_source_matches": True
            })
    manifest = {
        "revision": "2026-10-19", "verification": "Confronto blocco @startuml e metadati plantuml del PNG",
        "case_compliance": "APERTA: allineamento dei VPD storici alle viste correnti non verificato nel tool originale",
        "historical_vpd": "Non modificati, non dichiarati sincronizzati",
        "historical_deployment": "deployment_diagram.png escluso dalle viste finali correnti",
        "diagrams": records
    }
    (ROOT / "diagrammi_manifest.json").write_text(json.dumps(manifest, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    print(f"Sorgenti/PNG coerenti: {len(records)} viste. Allineamento VPD nel tool originale non verificato.")

if __name__ == "__main__":
    parser = argparse.ArgumentParser()
    parser.add_argument("--render-diagrams", action="store_true")
    parser.add_argument("--java", default="java")
    parser.add_argument("--plantuml")
    args = parser.parse_args()
    if args.render_diagrams:
        if not args.plantuml:
            parser.error("--plantuml richiesto con --render-diagrams")
        # Rigenerazione esplicita dei PNG correnti. Nessun VPD viene toccato.
        sources = [str(p) for p in DOC.rglob("*.puml") if "_build" not in p.parts]
        subprocess.run([args.java, "-Djava.awt.headless=true", "-jar", args.plantuml,
                        "-failfast2", "-tpng", *sources], check=True)
    prepare()
    verify_diagrams()
