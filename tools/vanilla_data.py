from __future__ import annotations

import csv
import json
import re
import xml.etree.ElementTree as ET
from collections import OrderedDict
from dataclasses import dataclass
from pathlib import Path
from typing import Any, Iterable

REPO_ROOT = Path(__file__).resolve().parents[1]
DATASET_ROOT = REPO_ROOT / "db" / "datasets" / "7dtd-vanilla"
DERIVED_ROOT = DATASET_ROOT / "derived"
RAW_ROOT = DATASET_ROOT / "raw"
DEFAULT_CONFIG_DIR = Path(
    "/mnt/d/Program Files (x86)/Steam/steamapps/common/7 Days To Die/Data/Config"
)

ENTITY_FILES = {
    "blocks": "blocks.json",
    "items": "items.json",
    "recipes": "recipes.json",
    "workstations": "workstations.json",
    "localization": "localization.json",
}

REQUIRED_ENTITY_FIELDS = {
    "blocks": ["id", "source_file"],
    "items": ["id", "source_file"],
    "recipes": ["id", "output", "source_file"],
    "workstations": ["id", "source_file"],
    "localization": ["key", "source_file"],
}


def ensure_dataset_dirs() -> None:
    DERIVED_ROOT.mkdir(parents=True, exist_ok=True)
    RAW_ROOT.mkdir(parents=True, exist_ok=True)


def load_json(path: Path, default: Any = None) -> Any:
    if not path.exists():
        return default
    return json.loads(path.read_text(encoding="utf-8"))


def dump_json(path: Path, data: Any) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(
        json.dumps(data, indent=2, ensure_ascii=False, sort_keys=True) + "\n",
        encoding="utf-8",
    )


def sorted_unique(records: list[dict[str, Any]], key: str) -> list[dict[str, Any]]:
    return sorted(records, key=lambda row: (str(row.get(key, "")), json.dumps(row, sort_keys=True, ensure_ascii=False)))


def as_int(value: Any) -> int | None:
    if value is None:
        return None
    text = str(value).strip()
    if text == "":
        return None
    try:
        return int(text)
    except ValueError:
        try:
            return int(float(text))
        except ValueError:
            return None


def as_number(value: Any) -> int | float | None:
    if value is None:
        return None
    text = str(value).strip()
    if text == "":
        return None
    if re.fullmatch(r"[-+]?\d+", text):
        return int(text)
    try:
        return float(text)
    except ValueError:
        return text


def bool_from_text(value: Any) -> bool | None:
    if value is None:
        return None
    text = str(value).strip().lower()
    if text in {"true", "1", "yes", "y"}:
        return True
    if text in {"false", "0", "no", "n"}:
        return False
    return None


def split_list(value: Any) -> list[str]:
    if value is None:
        return []
    text = str(value).strip()
    if not text:
        return []
    return [part.strip() for part in text.split(",") if part.strip()]


def first_non_player(value: Any) -> str | None:
    choices = split_list(value)
    for choice in choices:
        if choice and choice.lower() != "player":
            return choice
    return choices[0] if choices else None


def parse_property_section(section: ET.Element) -> dict[str, Any]:
    parsed: dict[str, Any] = {}
    for prop in section.findall("./property"):
        name = prop.get("name")
        if not name:
            continue
        parsed[name] = prop.get("value")
    return parsed


def parse_nested_properties(element: ET.Element) -> dict[str, dict[str, Any]]:
    nested: dict[str, dict[str, Any]] = {}
    for section in element.findall("./property[@class]"):
        class_name = section.get("class")
        if not class_name:
            continue
        nested[class_name] = parse_property_section(section)
    return nested


def parse_localization(path: Path) -> list[dict[str, Any]]:
    if not path.exists():
        return []
    rows: list[dict[str, Any]] = []
    with path.open("r", encoding="utf-8-sig", newline="") as handle:
        reader = csv.reader(handle)
        header = next(reader, None)
        if not header:
            return []
        for raw in reader:
            if not raw or not raw[0].strip():
                continue
            padded = list(raw) + [""] * (len(header) - len(raw))
            row = dict(zip(header, padded))
            key = row.get("Key", "").strip()
            if not key:
                continue
            rows.append(
                {
                    "key": key,
                    "category": row.get("File", "").strip() or None,
                    "type": row.get("Type", "").strip() or None,
                    "english": row.get("english", "").strip() or None,
                    "context": row.get("Context / Alternate Text", "").strip() or None,
                    "source_file": "Localization.txt",
                }
            )
    return sorted(rows, key=lambda row: row["key"])


def localization_index(entries: Iterable[dict[str, Any]]) -> dict[str, dict[str, Any]]:
    return {row["key"]: row for row in entries if row.get("key")}


def localization_lookup(key: str | None, loc_index: dict[str, dict[str, Any]]) -> str | None:
    if not key:
        return None
    row = loc_index.get(key)
    if row and row.get("english"):
        return row["english"]
    return None


def display_name_from_key(key: str | None, loc_index: dict[str, dict[str, Any]]) -> str | None:
    name = localization_lookup(key, loc_index)
    if name:
        return name
    if not key:
        return None
    return key


def parse_items(path: Path, loc_index: dict[str, dict[str, Any]]) -> list[dict[str, Any]]:
    if not path.exists():
        return []
    root = ET.parse(path).getroot()
    records: list[dict[str, Any]] = []
    for item in root.findall("./item"):
        item_id = item.get("name")
        if not item_id:
            continue
        props = {prop.get("name"): prop.get("value") for prop in item.findall("./property") if prop.get("name")}
        nested = parse_nested_properties(item)
        description_key = props.get("DescriptionKey")
        record = {
            "id": item_id,
            "display_name": display_name_from_key(item_id, loc_index),
            "source_file": "items.xml",
            "source_path": str(path),
            "description_key": description_key,
            "stack_size": as_int(props.get("Stacknumber")),
            "value": as_int(props.get("EconomicValue")),
            "material": props.get("Material"),
            "group": props.get("Group"),
            "creative_mode": props.get("CreativeMode"),
            "icon": props.get("CustomIcon"),
            "model": props.get("Meshfile"),
            "display_type": props.get("DisplayType"),
            "unlocked_by": props.get("UnlockedBy"),
            "tags": split_list(props.get("Tags")),
            "repair_items": _repair_items_from_section(nested.get("RepairItems", {})),
            "notes": _item_notes(props, nested),
        }
        records.append(record)
    return _dedupe_and_sort(records, "id")


def _item_notes(props: dict[str, Any], nested: dict[str, dict[str, Any]]) -> str | None:
    parts: list[str] = []
    if props.get("Extends"):
        parts.append(f"extends {props['Extends']}")
    if nested.get("RepairItems"):
        parts.append("has repair items")
    if props.get("DescriptionKey"):
        parts.append(f"desc:{props['DescriptionKey']}")
    return "; ".join(parts) or None


def _repair_items_from_section(section: dict[str, Any]) -> list[dict[str, Any]]:
    items: list[dict[str, Any]] = []
    for item_id, count in section.items():
        if item_id is None:
            continue
        items.append({"item": item_id, "count": as_number(count)})
    return sorted(items, key=lambda row: str(row["item"]))


def _dedupe_and_sort(records: list[dict[str, Any]], key: str) -> list[dict[str, Any]]:
    merged: OrderedDict[str, dict[str, Any]] = OrderedDict()
    for record in records:
        record_key = record.get(key)
        if record_key is None:
            continue
        if record_key in merged:
            merged[record_key].update({k: v for k, v in record.items() if v is not None})
        else:
            merged[record_key] = record
    return sorted(merged.values(), key=lambda row: str(row.get(key, "")))


def parse_blocks(path: Path, loc_index: dict[str, dict[str, Any]]) -> list[dict[str, Any]]:
    if not path.exists():
        return []
    root = ET.parse(path).getroot()
    records: list[dict[str, Any]] = []
    for block in root.findall("./block"):
        block_id = block.get("name")
        if not block_id:
            continue
        props = {prop.get("name"): prop.get("value") for prop in block.findall("./property") if prop.get("name")}
        nested = parse_nested_properties(block)
        workstation = nested.get("Workstation", {})
        repair = nested.get("RepairItems", {})
        crafting_area = first_non_player(workstation.get("CraftingAreaRecipes"))
        block_class = props.get("Class") or ("Workstation" if workstation else None)
        record = {
            "id": block_id,
            "display_name": display_name_from_key(block_id, loc_index),
            "source_file": "blocks.xml",
            "source_path": str(path),
            "block_class": block_class,
            "crafting_area": crafting_area,
            "crafting_areas": split_list(workstation.get("CraftingAreaRecipes")),
            "modules": split_list(workstation.get("Modules")),
            "input_materials": split_list(workstation.get("InputMaterials")),
            "tool_names": split_list(workstation.get("ToolNames")),
            "description_key": props.get("DescriptionKey"),
            "stack_size": as_int(props.get("Stacknumber")),
            "value": as_int(props.get("EconomicValue")),
            "material": props.get("Material"),
            "group": props.get("Group"),
            "creative_mode": props.get("CreativeMode"),
            "icon": props.get("WorkstationIcon") or props.get("CustomIcon"),
            "model": props.get("Model"),
            "unlocked_by": props.get("UnlockedBy"),
            "tags": split_list(props.get("Tags")),
            "filter_tags": split_list(props.get("FilterTags")),
            "repair_items": _repair_items_from_section(repair),
            "notes": _block_notes(props, nested),
        }
        records.append(record)
    return _dedupe_and_sort(records, "id")


def _block_notes(props: dict[str, Any], nested: dict[str, dict[str, Any]]) -> str | None:
    parts: list[str] = []
    if props.get("Extends"):
        parts.append(f"extends {props['Extends']}")
    if nested.get("Workstation"):
        parts.append("workstation")
    if nested.get("RepairItems"):
        parts.append("repair items")
    if props.get("DescriptionKey"):
        parts.append(f"desc:{props['DescriptionKey']}")
    return "; ".join(parts) or None


def parse_recipes(path: Path, loc_index: dict[str, dict[str, Any]]) -> list[dict[str, Any]]:
    if not path.exists():
        return []
    root = ET.parse(path).getroot()
    records: list[dict[str, Any]] = []
    for recipe in root.findall("./recipe"):
        output_id = recipe.get("name")
        if not output_id:
            continue
        ingredients = []
        for ingredient in recipe.findall("./ingredient"):
            name = ingredient.get("name")
            if not name:
                continue
            ingredients.append(
                {
                    "item": name,
                    "count": as_number(ingredient.get("count")),
                }
            )
        specials = [child.tag for child in recipe if child.tag != "ingredient"]
        record = {
            "id": output_id,
            "output": output_id,
            "output_display_name": display_name_from_key(output_id, loc_index),
            "count": as_number(recipe.get("count")),
            "craft_area": recipe.get("craft_area"),
            "craft_time": as_number(recipe.get("craft_time")),
            "craft_exp_gain": as_number(recipe.get("craft_exp_gain")),
            "always_unlocked": bool_from_text(recipe.get("always_unlocked")),
            "material_based": bool_from_text(recipe.get("material_based")),
            "is_trackable": bool_from_text(recipe.get("is_trackable")),
            "use_ingredient_modifier": bool_from_text(recipe.get("use_ingredient_modifier")),
            "tags": split_list(recipe.get("tags")),
            "tooltip": recipe.get("tooltip"),
            "ingredients": ingredients,
            "special_components": specials,
            "source_file": "recipes.xml",
            "source_path": str(path),
            "notes": _recipe_notes(recipe, specials),
        }
        records.append(record)
    return _dedupe_and_sort(records, "id")


def _recipe_notes(recipe: ET.Element, specials: list[str]) -> str | None:
    parts: list[str] = []
    if recipe.get("Extends"):
        parts.append(f"extends {recipe.get('Extends')}")
    if specials:
        parts.append("special:" + ",".join(sorted(set(specials))))
    if recipe.get("tooltip"):
        parts.append(f"tooltip:{recipe.get('tooltip')}")
    return "; ".join(parts) or None


def derive_workstations(blocks: list[dict[str, Any]]) -> list[dict[str, Any]]:
    records: list[dict[str, Any]] = []
    for block in blocks:
        if (block.get("block_class") or "").lower() != "workstation" and not block.get("crafting_area"):
            continue
        if (block.get("block_class") or "").lower() != "workstation" and not block.get("modules"):
            continue
        records.append(
            {
                "id": block["id"],
                "display_name": block.get("display_name"),
                "source_file": block.get("source_file"),
                "source_path": block.get("source_path"),
                "block_class": block.get("block_class"),
                "crafting_area": block.get("crafting_area"),
                "ingredients": block.get("repair_items", []),
                "description_key": block.get("description_key"),
                "icon": block.get("icon"),
                "model": block.get("model"),
                "modules": block.get("modules", []),
                "input_materials": block.get("input_materials", []),
                "tool_names": block.get("tool_names", []),
                "unlocked_by": block.get("unlocked_by"),
                "notes": block.get("notes"),
            }
        )
    return _dedupe_and_sort(records, "id")


def build_dataset(config_dir: Path | None = None) -> dict[str, list[dict[str, Any]]]:
    config_dir = config_dir or DEFAULT_CONFIG_DIR
    localization = parse_localization(config_dir / "Localization.txt")
    loc_index = localization_index(localization)
    items = parse_items(config_dir / "items.xml", loc_index)
    blocks = parse_blocks(config_dir / "blocks.xml", loc_index)
    recipes = parse_recipes(config_dir / "recipes.xml", loc_index)
    workstations = derive_workstations(blocks)
    return {
        "localization": localization,
        "items": items,
        "blocks": blocks,
        "recipes": recipes,
        "workstations": workstations,
    }


def write_dataset(dataset: dict[str, list[dict[str, Any]]]) -> None:
    ensure_dataset_dirs()
    for entity, filename in ENTITY_FILES.items():
        dump_json(DERIVED_ROOT / filename, dataset.get(entity, []))


def load_dataset_from_derived(dataset_root: Path = DATASET_ROOT) -> dict[str, list[dict[str, Any]]]:
    derived_root = dataset_root / "derived"
    loaded: dict[str, list[dict[str, Any]]] = {}
    for entity, filename in ENTITY_FILES.items():
        loaded[entity] = load_json(derived_root / filename, default=[])
    return loaded


def record_matches(record: dict[str, Any], needle: str) -> bool:
    needle_l = needle.lower()
    for value in record.values():
        if isinstance(value, str) and needle_l in value.lower():
            return True
        if isinstance(value, list):
            for item in value:
                if isinstance(item, str) and needle_l in item.lower():
                    return True
                if isinstance(item, dict):
                    for v in item.values():
                        if isinstance(v, str) and needle_l in v.lower():
                            return True
                        if isinstance(v, (int, float)) and needle_l in str(v).lower():
                            return True
        if isinstance(value, dict):
            for v in value.values():
                if isinstance(v, str) and needle_l in v.lower():
                    return True
                if isinstance(v, (int, float)) and needle_l in str(v).lower():
                    return True
        if isinstance(value, (int, float)) and needle_l in str(value).lower():
            return True
    return False


def validate_entity_records(entity: str, records: list[dict[str, Any]]) -> list[str]:
    issues: list[str] = []
    if not isinstance(records, list):
        return [f"{entity}: expected list, found {type(records).__name__}"]
    required_fields = REQUIRED_ENTITY_FIELDS.get(entity, [])
    ids: list[str] = []
    for index, record in enumerate(records):
        if not isinstance(record, dict):
            issues.append(f"{entity}[{index}]: expected object, found {type(record).__name__}")
            continue
        for field in required_fields:
            if field not in record or record[field] in {None, ""}:
                issues.append(f"{entity}[{index}]: missing required field {field}")
        record_id = record.get("id") or record.get("key")
        if record_id is not None:
            ids.append(str(record_id))
    if ids and ids != sorted(ids):
        issues.append(f"{entity}: records are not sorted by id/key")
    if len(ids) != len(set(ids)):
        issues.append(f"{entity}: duplicate ids/keys found")
    return issues


def validate_dataset(dataset: dict[str, list[dict[str, Any]]]) -> list[str]:
    issues: list[str] = []
    for entity, records in dataset.items():
        issues.extend(validate_entity_records(entity, records))
    return issues


def print_record_summary(entity: str, record: dict[str, Any]) -> str:
    if entity in {"items", "blocks", "workstations"}:
        parts = [record.get("id"), record.get("display_name")]
        if record.get("crafting_area"):
            parts.append(f"craft_area={record.get('crafting_area')}")
        if record.get("stack_size") is not None:
            parts.append(f"stack={record.get('stack_size')}")
        if record.get("value") is not None:
            parts.append(f"value={record.get('value')}")
        return " | ".join(str(part) for part in parts if part is not None)
    if entity == "recipes":
        output = record.get("output_display_name") or record.get("output")
        return f"{record.get('id')} -> {output} | area={record.get('craft_area')} | count={record.get('count')}"
    if entity == "localization":
        return f"{record.get('key')} -> {record.get('english')}"
    return json.dumps(record, ensure_ascii=False, sort_keys=True)


def query_dataset(dataset: dict[str, list[dict[str, Any]]], entity: str, needle: str, limit: int = 20) -> list[dict[str, Any]]:
    records = dataset.get(entity, [])
    matches = [row for row in records if record_matches(row, needle)]
    return matches[:limit]
