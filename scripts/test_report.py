#!/usr/bin/env python3
"""Отчёт о тестировании для курсового проекта ParkingManager.

Объединяет результаты всех запусков тестов (файлы .trx) и сведения
из исходного кода тестов (атрибуты [Trait("Category", ...)] и
[Trait("Task", ...)]) и формирует Markdown-отчёт:

  * сводка по видам тестов (модульные, интеграционные, UI);
  * матрица трассируемости «задача → тесты → результат», которая
    показывает, какие задачи из плана подтверждены автотестами;
  * список упавших тестов с сообщениями об ошибках;
  * полный перечень тестов.

Пример запуска:
  python scripts/test_report.py --results TestResults --sources tests \
      --tasks tests/tasks.json --out TestResults/test-report.md
"""

import argparse
import datetime
import glob
import json
import os
import re
import sys
import xml.etree.ElementTree as ET

TRX_NS = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}

CATEGORY_TITLES = {
    "Unit": "Модульные",
    "Integration": "Интеграционные",
    "UI": "От лица пользователя (UI, FlaUI)",
}

OUTCOME_TITLES = {
    "Passed": "✅ пройден",
    "Failed": "❌ упал",
    "NotRun": "⏸ не запускался",
    "Skipped": "⏭ пропущен",
}

TRAIT_RE = re.compile(r'\[Trait\(\s*"(?P<key>Category|Task)"\s*,\s*"(?P<value>[^"]+)"\s*\)\]')
CLASS_RE = re.compile(r"\bclass\s+(?P<name>\w+)")
METHOD_RE = re.compile(r"\bpublic\s+(?:async\s+)?(?:void|Task)\s+(?P<name>\w+)\s*\(")
TEST_ATTR_RE = re.compile(r"\[(?:Fact|Theory)\b")


def parse_sources(sources_dir):
    """Возвращает {(Класс, Метод): {"category": str, "tasks": set, "file": str}}."""
    tests = {}
    for path in sorted(glob.glob(os.path.join(sources_dir, "**", "*.cs"), recursive=True)):
        if os.sep + "obj" + os.sep in path or os.sep + "bin" + os.sep in path:
            continue
        with open(path, encoding="utf-8-sig") as f:
            lines = f.readlines()

        class_name = None
        class_traits = {"Category": [], "Task": []}
        pending = {"Category": [], "Task": []}
        pending_is_test = False

        for line in lines:
            for m in TRAIT_RE.finditer(line):
                pending[m.group("key")].append(m.group("value").strip())
            if TEST_ATTR_RE.search(line):
                pending_is_test = True

            class_match = CLASS_RE.search(line)
            if class_match and not line.lstrip().startswith("//"):
                class_name = class_match.group("name")
                class_traits = pending
                pending = {"Category": [], "Task": []}
                pending_is_test = False
                continue

            method_match = METHOD_RE.search(line)
            if method_match and class_name:
                if pending_is_test:
                    categories = pending["Category"] or class_traits["Category"]
                    tasks = set(class_traits["Task"]) | set(pending["Task"])
                    tests[(class_name, method_match.group("name"))] = {
                        "category": categories[0] if categories else "Other",
                        "tasks": tasks,
                        "file": os.path.relpath(path, sources_dir).replace(os.sep, "/"),
                    }
                pending = {"Category": [], "Task": []}
                pending_is_test = False
    return tests


def parse_duration(value):
    if not value:
        return 0.0
    hours, minutes, seconds = value.split(":")
    return int(hours) * 3600 + int(minutes) * 60 + float(seconds)


def test_key(test_name):
    """'Ns.Class.Method(a: 1)' -> ('Class', 'Method')."""
    base = test_name.split("(", 1)[0]
    parts = base.split(".")
    if len(parts) < 2:
        return ("", base)
    return (parts[-2], parts[-1])


def parse_results(results_dir):
    """Список результатов: dict(key, name, outcome, duration, message)."""
    results = []
    seen = set()
    for path in sorted(glob.glob(os.path.join(results_dir, "**", "*.trx"), recursive=True)):
        root = ET.parse(path).getroot()
        for r in root.iterfind(".//t:Results/t:UnitTestResult", TRX_NS):
            name = r.get("testName", "")
            # Один и тот же тест может попасть в отчёт из нескольких артефактов.
            ident = (name, r.get("executionId"))
            if ident in seen:
                continue
            seen.add(ident)

            outcome = r.get("outcome", "NotRun")
            if outcome not in OUTCOME_TITLES:
                outcome = "Skipped" if outcome in ("NotExecuted", "Inconclusive") else "Failed"
            message_node = r.find("./t:Output/t:ErrorInfo/t:Message", TRX_NS)
            results.append({
                "key": test_key(name),
                "name": name,
                "outcome": outcome,
                "duration": parse_duration(r.get("duration")),
                "message": (message_node.text or "").strip() if message_node is not None else "",
            })
    return results


def md_escape(text):
    return text.replace("|", "\\|").replace("\n", " ")


def build_report(sources, results, tasks, title):
    lines = []
    sha = os.environ.get("GITHUB_SHA", "")[:7]
    ref = os.environ.get("GITHUB_HEAD_REF") or os.environ.get("GITHUB_REF_NAME", "")
    now = datetime.datetime.now(datetime.timezone.utc).strftime("%Y-%m-%d %H:%M UTC")
    lines.append(f"# {title}")
    lines.append("")
    meta = [f"Сформирован: {now}"]
    if ref:
        meta.append(f"ветка `{ref}`")
    if sha:
        meta.append(f"коммит `{sha}`")
    lines.append(", ".join(meta) + ".")
    lines.append("")

    # Привязка результатов к описаниям тестов из исходного кода.
    for r in results:
        info = sources.get(r["key"], {})
        r["category"] = info.get("category", "Other")
        r["tasks"] = info.get("tasks", set())

    # 1. Сводка по видам тестов.
    lines.append("## 1. Сводка по видам тестов")
    lines.append("")
    lines.append("| Вид тестов | Всего | Пройдено | Упало | Пропущено | Время, с |")
    lines.append("|---|---:|---:|---:|---:|---:|")
    order = list(CATEGORY_TITLES) + sorted({r["category"] for r in results} - set(CATEGORY_TITLES))
    total = {"all": 0, "Passed": 0, "Failed": 0, "Skipped": 0, "time": 0.0}
    for category in order:
        group = [r for r in results if r["category"] == category]
        if not group:
            continue
        passed = sum(r["outcome"] == "Passed" for r in group)
        failed = sum(r["outcome"] == "Failed" for r in group)
        skipped = len(group) - passed - failed
        duration = sum(r["duration"] for r in group)
        total["all"] += len(group)
        total["Passed"] += passed
        total["Failed"] += failed
        total["Skipped"] += skipped
        total["time"] += duration
        lines.append(f"| {CATEGORY_TITLES.get(category, category)} | {len(group)} | {passed} | {failed} | {skipped} | {duration:.1f} |")
    lines.append(f"| **Итого** | **{total['all']}** | **{total['Passed']}** | **{total['Failed']}** | **{total['Skipped']}** | **{total['time']:.1f}** |")
    lines.append("")
    if not results:
        lines.append("> Результаты тестов не найдены.")
        lines.append("")

    # 2. Трассируемость задач.
    lines.append("## 2. Подтверждение задач проекта тестами")
    lines.append("")
    lines.append("| № | Задача | Тестов | Пройдено | Упало | Статус |")
    lines.append("|---:|---|---:|---:|---:|---|")
    for task in tasks:
        tid = task["id"]
        group = [r for r in results if tid in r["tasks"]]
        passed = sum(r["outcome"] == "Passed" for r in group)
        failed = sum(r["outcome"] == "Failed" for r in group)
        kind = task.get("kind", "impl")
        if kind == "doc":
            status = "📄 документ, проверяется преподавателем"
        elif kind == "infra":
            group = results
            passed = total["Passed"]
            failed = total["Failed"]
            status = "⚙️ инфраструктура тестирования (данный отчёт)"
        elif not group:
            status = "⏳ автотестов нет — задача ещё не реализована"
        elif failed:
            status = "❌ есть упавшие тесты"
        elif passed == len(group):
            status = "✅ подтверждена"
        else:
            status = "⚠️ часть тестов не запускалась"
        if task.get("note"):
            status += f" — {task['note']}"
        count = len(group) if kind != "doc" else "—"
        p = passed if kind != "doc" else "—"
        fl = failed if kind != "doc" else "—"
        lines.append(f"| {tid} | {md_escape(task['name'])} | {count} | {p} | {fl} | {status} |")
    lines.append("")
    untagged = sorted({r["name"] for r in results if not r["tasks"]})
    if untagged:
        lines.append(f"> Тестов без привязки к задаче: {len(untagged)}. Добавьте атрибут `[Trait(\"Task\", \"N\")]`.")
        lines.append("")

    # 3. Упавшие тесты.
    failed_tests = [r for r in results if r["outcome"] == "Failed"]
    lines.append("## 3. Упавшие тесты")
    lines.append("")
    if failed_tests:
        lines.append("| Тест | Сообщение |")
        lines.append("|---|---|")
        for r in failed_tests:
            lines.append(f"| `{md_escape(r['name'])}` | {md_escape(r['message'][:300])} |")
    else:
        lines.append("Нет.")
    lines.append("")

    # 4. Полный перечень.
    lines.append("## 4. Перечень тестов")
    lines.append("")
    lines.append("<details><summary>Показать все тесты</summary>")
    lines.append("")
    lines.append("| Тест | Вид | Задачи | Результат | Время, с |")
    lines.append("|---|---|---|---|---:|")
    for r in sorted(results, key=lambda x: (x["category"], x["name"])):
        tasks_text = ", ".join(sorted(r["tasks"], key=lambda t: int(t) if t.isdigit() else 0)) or "—"
        lines.append(
            f"| `{md_escape(r['name'])}` | {CATEGORY_TITLES.get(r['category'], r['category'])} | "
            f"{tasks_text} | {OUTCOME_TITLES[r['outcome']]} | {r['duration']:.2f} |")
    lines.append("")
    lines.append("</details>")
    lines.append("")
    return "\n".join(lines), total


def main():
    ap = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    ap.add_argument("--results", default="TestResults", help="каталог с файлами .trx (поиск рекурсивный)")
    ap.add_argument("--sources", default="tests", help="каталог с исходным кодом тестов")
    ap.add_argument("--tasks", default="tests/tasks.json", help="список задач проекта")
    ap.add_argument("--title", default="Отчёт о тестировании ParkingManager")
    ap.add_argument("--out", help="файл для сохранения отчёта (Markdown)")
    ap.add_argument("--summary", help="файл, в конец которого дописать отчёт (например, $GITHUB_STEP_SUMMARY)")
    ap.add_argument("--fail-on-failed", action="store_true", help="код возврата 1, если есть упавшие тесты")
    args = ap.parse_args()

    with open(args.tasks, encoding="utf-8") as f:
        tasks = json.load(f)["tasks"]

    report, total = build_report(parse_sources(args.sources), parse_results(args.results), tasks, args.title)

    if args.out:
        os.makedirs(os.path.dirname(os.path.abspath(args.out)), exist_ok=True)
        with open(args.out, "w", encoding="utf-8") as f:
            f.write(report)
    if args.summary:
        with open(args.summary, "a", encoding="utf-8") as f:
            f.write(report + "\n")
    if not args.out and not args.summary:
        sys.stdout.reconfigure(encoding="utf-8")
        print(report)

    if args.fail_on_failed and total["Failed"]:
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
