#!/usr/bin/env python3
"""Dependency-free static checks for the KMK Unity project."""
from __future__ import annotations

import json
import re
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

REQUIRED = [
    "ProjectSettings/ProjectVersion.txt",
    "ProjectSettings/EditorBuildSettings.asset",
    "Packages/manifest.json",
    "Assets/KMK/Scenes/KMKMain.unity",
    "Assets/KMK/Runtime/KmkGame.cs",
    "Assets/KMK/Runtime/RunnerController.cs",
    "Assets/KMK/Runtime/RunnerAvatar.cs",
    "Assets/KMK/Runtime/KmkWorld.cs",
    "Assets/KMK/Runtime/KmkTrackSegment.cs",
    "Assets/KMK/Runtime/KmkCameraRig.cs",
    "Assets/KMK/Runtime/ProceduralAudio.cs",
    "Assets/KMK/Runtime/PremiumHud.cs",
    "Assets/KMK/Editor/KmkProjectSetup.cs",
]

SECRET_PATTERNS = [
    re.compile(r"-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----"),
    re.compile(r"(?i)\b(?:api[_-]?key|secret|token)\s*[:=]\s*['\"][A-Za-z0-9_\-]{20,}"),
    re.compile(r"\bsk-[A-Za-z0-9_-]{20,}"),
]


def fail(message: str) -> None:
    print(f"ERROR: {message}", file=sys.stderr)
    raise SystemExit(1)


def strip_csharp_comments_and_strings(source: str) -> str:
    # Preserve newlines while removing syntax that can contain braces.
    source = re.sub(r"/\*.*?\*/", lambda m: "\n" * m.group(0).count("\n"), source, flags=re.S)
    source = re.sub(r"//[^\n]*", "", source)
    source = re.sub(r'@"(?:""|[^"])*"', '""', source)
    source = re.sub(r'\$?"(?:\\.|[^"\\])*"', '""', source)
    source = re.sub(r"'(?:\\.|[^'\\])'", "''", source)
    return source


def check_balanced(path: Path) -> None:
    cleaned = strip_csharp_comments_and_strings(path.read_text(encoding="utf-8"))
    pairs = {"{": "}", "(": ")", "[": "]"}
    closing = {v: k for k, v in pairs.items()}
    stack: list[tuple[str, int]] = []
    for line_number, line in enumerate(cleaned.splitlines(), 1):
        for char in line:
            if char in pairs:
                stack.append((char, line_number))
            elif char in closing:
                if not stack or stack[-1][0] != closing[char]:
                    fail(f"unbalanced {char!r} in {path.relative_to(ROOT)} line {line_number}")
                stack.pop()
    if stack:
        char, line = stack[-1]
        fail(f"unclosed {char!r} in {path.relative_to(ROOT)} from line {line}")


def main() -> None:
    missing = [path for path in REQUIRED if not (ROOT / path).is_file()]
    if missing:
        fail("missing required files: " + ", ".join(missing))

    version = (ROOT / "ProjectSettings/ProjectVersion.txt").read_text(encoding="utf-8")
    if "6000.3.17f1" not in version:
        fail("ProjectVersion.txt must pin Unity 6000.3.17f1")

    manifest = json.loads((ROOT / "Packages/manifest.json").read_text(encoding="utf-8"))
    dependencies = manifest.get("dependencies", {})
    if "com.unity.ugui" not in dependencies:
        fail("com.unity.ugui is required by PremiumHud")

    editor = (ROOT / "Assets/KMK/Editor/KmkProjectSetup.cs").read_text(encoding="utf-8")
    for required in ["com.kmkparis.theessencerun", "BuildTarget.iOS", "ScriptingImplementation.IL2CPP"]:
        if required not in editor and required not in (ROOT / "Assets/KMK/Runtime/KmkConstants.cs").read_text(encoding="utf-8"):
            fail(f"iOS setup is missing {required}")

    all_text = []
    for path in ROOT.rglob("*"):
        if not path.is_file() or any(part in {"Library", "Temp", "Build", "Builds"} for part in path.parts):
            continue
        if path.suffix.lower() in {".cs", ".json", ".md", ".txt", ".asset", ".unity", ".yml", ".yaml", ".meta"} or path.name in {".gitignore", ".gitattributes"}:
            text = path.read_text(encoding="utf-8")
            all_text.append((path, text))
            if path.suffix == ".cs":
                check_balanced(path)

    for path, text in all_text:
        for pattern in SECRET_PATTERNS:
            if pattern.search(text):
                fail(f"possible secret found in {path.relative_to(ROOT)}")

    class_names = set()
    enum_names = set()
    for path, text in all_text:
        if path.suffix != ".cs":
            continue
        class_names.update(re.findall(r"\bclass\s+([A-Za-z_][A-Za-z0-9_]*)", text))
        enum_names.update(re.findall(r"\benum\s+([A-Za-z_][A-Za-z0-9_]*)", text))

    expected_types = {
        "KmkGame", "RunnerController", "RunnerAvatar", "KmkWorld", "KmkTrackSegment",
        "KmkCameraRig", "ProceduralAudio", "PremiumHud", "EssenceCollectible", "KmkHazard",
        "KmkGameState", "KmkChapter", "HazardKind",
    }
    defined = class_names | enum_names
    absent = sorted(expected_types - defined)
    if absent:
        fail("missing gameplay types: " + ", ".join(absent))

    print(f"OK: {len([p for p, _ in all_text if p.suffix == '.cs'])} C# files checked")
    print("OK: Unity version, UGUI dependency, iOS identity and project structure validated")
    print("OK: no obvious committed secrets detected")


if __name__ == "__main__":
    main()
