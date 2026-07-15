#!/usr/bin/env python3
"""
HorosCode web image optimizer — CLI wrapper around image_utils.ImageUtils.

Resize, convert to WebP, optimize for web delivery, and generate responsive
srcset variants. No API keys required (Pillow only).

Usage:
  python optimize_web_image.py --input hero.png --out hero.webp
  python optimize_web_image.py --input hero.png --optimize
  python optimize_web_image.py --input hero.png --responsive
  python optimize_web_image.py --input hero.png --og --out public/images/og-hero.webp
  python optimize_web_image.py --input icon.png --resize 512 --format webp
  python optimize_web_image.py --input photo.jpg --info

Exit codes:
  0  success
  1  processing error
  2  invalid arguments / missing input file
  3  missing dependency (Pillow)
"""

from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import List, Optional, Tuple

# Resolve image_utils from references (no logic duplication)
_SCRIPT_DIR = Path(__file__).resolve().parent
_IMAGE_UTILS_DIR = _SCRIPT_DIR.parent / "references" / "code-examples"
if str(_IMAGE_UTILS_DIR) not in sys.path:
    sys.path.insert(0, str(_IMAGE_UTILS_DIR))

try:
    from image_utils import ImageUtils
except ImportError as exc:
    print(
        "ERROR: Could not import image_utils. Install dependencies:\n"
        "  pip install Pillow requests",
        file=sys.stderr,
    )
    print(f"  ({exc})", file=sys.stderr)
    sys.exit(3)

try:
    from PIL import Image
except ImportError:
    print(
        "ERROR: Pillow is not installed. Run: pip install Pillow requests",
        file=sys.stderr,
    )
    sys.exit(3)


DEFAULT_RESPONSIVE_WIDTHS = (640, 960, 1280, 1920)
OG_WIDTH = 1200
OG_HEIGHT = 630
DEFAULT_QUALITY = 85
DEFAULT_MAX_DIMENSION = 1920


def die(msg: str, code: int = 1) -> None:
    print(f"ERROR: {msg}", file=sys.stderr)
    sys.exit(code)


def parse_size(value: str) -> Tuple[Optional[int], Optional[int]]:
    """Parse '800' or '800x600' into (width, height)."""
    value = value.strip().lower()
    if "x" in value:
        parts = value.split("x", 1)
        if len(parts) != 2:
            die(f"Invalid size format: {value!r}", 2)
        try:
            return int(parts[0]), int(parts[1])
        except ValueError:
            die(f"Invalid size dimensions: {value!r}", 2)
    try:
        return int(value), None
    except ValueError:
        die(f"Invalid size: {value!r}", 2)


def default_output_path(
    input_path: Path,
    out: Optional[str],
    suffix: str = "",
    ext: str = ".webp",
) -> Path:
    if out:
        return Path(out)
    stem = input_path.stem + suffix
    return input_path.parent / f"{stem}{ext}"


def responsive_output_path(base_out: Path, width: int) -> Path:
    return base_out.parent / f"{base_out.stem}-{width}w{base_out.suffix}"


def load_input(path: Path) -> Image.Image:
    if not path.is_file():
        die(f"Input file not found: {path}", 2)
    try:
        return ImageUtils.load(path)
    except Exception as exc:
        die(f"Failed to load image: {exc}", 1)


def save_image(
    image: Image.Image,
    path: Path,
    quality: int,
) -> Path:
    try:
        ImageUtils.save(image, path, quality=quality)
        return path.resolve()
    except Exception as exc:
        die(f"Failed to save {path}: {exc}", 1)


def apply_resize(
    image: Image.Image,
    width: Optional[int],
    height: Optional[int],
    maintain_aspect: bool,
) -> Image.Image:
    if width is None and height is None:
        return image
    if maintain_aspect and width is not None and height is not None:
        return ImageUtils.resize(image, width, height, maintain_aspect=True)
    return ImageUtils.resize(image, width=width, height=height)


def run_optimize(
    image: Image.Image,
    max_dimension: int,
    fmt: str,
    quality: int,
    out_path: Path,
) -> Path:
    optimized_bytes = ImageUtils.optimize_for_web(
        image,
        max_dimension=max_dimension,
        format=fmt.upper(),
        quality=quality,
    )
    out_path.parent.mkdir(parents=True, exist_ok=True)
    out_path.write_bytes(optimized_bytes)
    return out_path.resolve()


def run_responsive(
    image: Image.Image,
    widths: List[int],
    out_path: Path,
    quality: int,
    fmt: str,
) -> List[Path]:
    written: List[Path] = []
    ext = out_path.suffix or ".webp"
    for width in widths:
        resized = ImageUtils.resize(image, width=width)
        target = responsive_output_path(
            out_path if out_path.suffix else out_path.with_suffix(ext),
            width,
        )
        if fmt.upper() == "WEBP":
            data = ImageUtils.optimize_for_web(
                resized, max_dimension=width, format="WEBP", quality=quality
            )
            target.parent.mkdir(parents=True, exist_ok=True)
            target.write_bytes(data)
        else:
            save_image(resized, target, quality)
        written.append(target.resolve())
    return written


def build_parser() -> argparse.ArgumentParser:
    p = argparse.ArgumentParser(
        description=(
            "HorosCode web image optimizer — resize, WebP, optimize_for_web, "
            "responsive variants. Wraps image_utils.ImageUtils (Pillow)."
        ),
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog=(
            "Examples:\n"
            "  %(prog)s -i hero.png --optimize\n"
            "  %(prog)s -i hero.png --responsive -o public/images/hero.webp\n"
            "  %(prog)s -i hero.png --og -o public/images/og-hero.webp\n"
            "  %(prog)s -i icon.png --resize 512 --format webp -o public/icons/icon.webp\n"
            "  %(prog)s -i photo.jpg --resize 1200x630 --quality 90\n"
        ),
    )
    p.add_argument(
        "--input",
        "-i",
        required=True,
        help="Input image path (PNG, JPEG, WebP, …)",
    )
    p.add_argument(
        "--out",
        "-o",
        help="Output file path (default: <input-stem>.webp beside input)",
    )
    p.add_argument(
        "--format",
        "-f",
        default="webp",
        choices=["webp", "jpeg", "jpg", "png"],
        help="Output format (default: webp)",
    )
    p.add_argument(
        "--quality",
        "-q",
        type=int,
        default=DEFAULT_QUALITY,
        help=f"Lossy quality 1–100 (default: {DEFAULT_QUALITY})",
    )
    p.add_argument(
        "--resize",
        metavar="SIZE",
        help="Resize to WIDTH, HEIGHT, or WIDTHxHEIGHT (e.g. 1200 or 1200x630)",
    )
    p.add_argument(
        "--maintain-aspect",
        action="store_true",
        help="When --resize has both dimensions, fit within box (letterbox-free)",
    )
    p.add_argument(
        "--optimize",
        action="store_true",
        help="Run optimize_for_web (resize if > max-dimension, compress)",
    )
    p.add_argument(
        "--max-dimension",
        type=int,
        default=DEFAULT_MAX_DIMENSION,
        help=f"Max width/height for --optimize (default: {DEFAULT_MAX_DIMENSION})",
    )
    p.add_argument(
        "--responsive",
        action="store_true",
        help=(
            "Emit multiple widths for srcset "
            f"(default: {', '.join(map(str, DEFAULT_RESPONSIVE_WIDTHS))}px)"
        ),
    )
    p.add_argument(
        "--responsive-widths",
        metavar="W,W,...",
        help="Comma-separated widths for --responsive (overrides defaults)",
    )
    p.add_argument(
        "--og",
        action="store_true",
        help=f"Crop to Open Graph aspect {OG_WIDTH}×{OG_HEIGHT} then optimize",
    )
    p.add_argument(
        "--info",
        action="store_true",
        help="Print image metadata as JSON and exit (no write)",
    )
    p.add_argument(
        "--json",
        action="store_true",
        help="On success, print result paths as JSON (for agents)",
    )
    return p


def main() -> int:
    parser = build_parser()
    args = parser.parse_args()

    input_path = Path(args.input).resolve()
    image = load_input(input_path)

    if args.info:
        info = ImageUtils.get_info(image)
        info["path"] = str(input_path)
        print(json.dumps(info, indent=2))
        return 0

    fmt = args.format.lower()
    if fmt == "jpg":
        fmt = "jpeg"
    ext = ".webp" if fmt == "webp" else f".{fmt}"

    outputs: List[Path] = []

    # OG preset: crop 1200:630 then optimize
    if args.og:
        cropped = ImageUtils.crop_to_aspect(image, f"{OG_WIDTH}:{OG_HEIGHT}")
        resized = ImageUtils.resize(cropped, width=OG_WIDTH, height=OG_HEIGHT)
        out_path = default_output_path(
            input_path, args.out, suffix="-og", ext=ext
        )
        outputs.append(
            run_optimize(resized, OG_WIDTH, "WEBP", args.quality, out_path)
        )
        _report(outputs, args.json)
        return 0

    # Responsive variants
    if args.responsive:
        if args.responsive_widths:
            try:
                widths = [int(w.strip()) for w in args.responsive_widths.split(",")]
            except ValueError:
                die("Invalid --responsive-widths; use comma-separated integers", 2)
        else:
            widths = list(DEFAULT_RESPONSIVE_WIDTHS)

        base_out = default_output_path(input_path, args.out, ext=ext)
        outputs.extend(
            run_responsive(image, widths, base_out, args.quality, fmt)
        )
        _report(outputs, args.json)
        return 0

    # Single-file pipeline
    working = image

    if args.resize:
        width, height = parse_size(args.resize)
        working = apply_resize(working, width, height, args.maintain_aspect)

    out_path = default_output_path(input_path, args.out, ext=ext)

    if args.optimize:
        outputs.append(
            run_optimize(
                working,
                args.max_dimension,
                fmt.upper() if fmt != "jpeg" else "JPEG",
                args.quality,
                out_path,
            )
        )
    else:
        outputs.append(save_image(working, out_path, args.quality))

    _report(outputs, args.json)
    return 0


def _report(paths: List[Path], as_json: bool) -> None:
    if as_json:
        print(json.dumps({"outputs": [str(p) for p in paths]}, indent=2))
    else:
        for p in paths:
            size_kb = p.stat().st_size / 1024
            print(f"OK: {p} ({size_kb:.1f} KB)")


if __name__ == "__main__":
    try:
        sys.exit(main())
    except KeyboardInterrupt:
        print("\nInterrupted.", file=sys.stderr)
        sys.exit(1)
