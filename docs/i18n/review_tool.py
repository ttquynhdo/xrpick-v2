"""EN-DE review export / import for the native German reviewer (backlog #13, decision 002).

Source of truth for texts: docs/i18n/strings-todo.csv (key, context, en, de, max_chars).
Review state:               docs/i18n/review-status.csv (key, de_text, status, reviewer, date, comment).

  python docs/i18n/review_tool.py export [--date YYYY-MM-DD]
      -> docs/i18n/review/review-<date>.csv (UTF-8 with BOM, opens in Excel with umlauts)
         docs/i18n/review/review-<date>-ANLEITUNG.md (German instructions for the reviewer)

  python docs/i18n/review_tool.py import <file> --reviewer "<Name>" [--date YYYY-MM-DD] [--dry-run]
      Accepts ',' or ';' as separator, UTF-8 (with/without BOM) or Windows-1252 (German Excel).
      Rejects the whole file if it contains the replacement character U+FFFD.
      Per row: OK? = ja and no correction -> confirmed;
               a Korrekturvorschlag (or OK? = nein with one) -> DE text replaced in strings-todo.csv, confirmed,
               comment "korrigiert durch Reviewer" (run the changed keys through german-reviewer afterwards,
               then LocalizationBinder.ImportCsv() in Unity);
               placeholders {name} or rich-text tags that differ from EN -> error, row left unchanged.
      Exit code 1 if any row was rejected.

Only cell values are read from a returned file; nothing in it is executed.
Standard library only.
"""
import argparse
import csv
import datetime
import io
import os
import re
import sys

HERE = os.path.dirname(os.path.abspath(__file__))
STRINGS = os.path.join(HERE, "strings-todo.csv")
STATUS = os.path.join(HERE, "review-status.csv")
OUT_DIR = os.path.join(HERE, "review")

COLUMNS = ["Key", "Kontext (Wo erscheint der Text?)", "English", "Deutsch", "Status",
           "OK? (ja/nein)", "Korrekturvorschlag", "Kommentar"]
STATUS_HEADER = ["key", "de_text", "status", "reviewer", "date", "comment"]
# Course order, so the reviewer reads the texts in the order a trainee meets them.
SCENE_ORDER = ["entry", "welcome", "menu", "lesson", "common", "concept", "basic", "mixed",
               "complex", "order", "end", "label", "wms"]
SCENE_NAME = {"entry": "Willkommen (Code-Eingabe)", "welcome": "1. Einführung", "menu": "Menü",
              "lesson": "Menü (Lektionen)", "common": "mehrere Lektionen", "concept": "2. Grundlagen",
              "basic": "3. Kommissionieren nach EKZ 3", "mixed": "4. Mehrere Positionen",
              "complex": "5. Volle Kartons und loses Material", "order": "Rückmeldungen (3.–5.)",
              "end": "Sitzung beenden", "label": "Etiketten", "wms": "LVS"}

PLACEHOLDER = re.compile(r"\{[A-Za-z_][A-Za-z0-9_]*\}")
TAG = re.compile(r"</?[A-Za-z]+(?:=[^>]*)?>")


def read_csv_file(path):
    with open(path, encoding="utf-8-sig", newline="") as f:
        return list(csv.reader(f))


def is_pseudo(en):
    return (en.startswith("=") and en.endswith(".*")) or en.startswith("(")


def load_strings():
    rows = read_csv_file(STRINGS)
    header = rows[0]
    return header, [r for r in rows[1:] if r and r[0]]


def load_status():
    if not os.path.exists(STATUS):
        return {}, []
    rows = read_csv_file(STATUS)
    order = [r[0] for r in rows[1:] if r and r[0]]
    return {r[0]: r + [""] * (6 - len(r)) for r in rows[1:] if r and r[0]}, order


def scene_rank(key):
    prefix = key.split(".")[0]
    return SCENE_ORDER.index(prefix) if prefix in SCENE_ORDER else len(SCENE_ORDER)


def export(date):
    _, strings = load_strings()
    status, _ = load_status()
    rows = []
    for r in strings:
        key, context, en, de = (r + [""] * 4)[:4]
        if is_pseudo(en) or key.startswith("label."):
            continue  # same rows LocalizationBinder skips (package labels are static WMS data)
        st = status.get(key, [key, de, "proposed"])[2] or "proposed"
        # A confirmed row whose DE text changed since is open again.
        if st == "confirmed" and key in status and status[key][1] != de:
            st = "proposed"
        rows.append((scene_rank(key), 0 if st == "proposed" else 1, key,
                     [key, context, en, de, st, "", "", ""]))
    rows.sort(key=lambda x: (x[0], x[1], x[2]))
    os.makedirs(OUT_DIR, exist_ok=True)
    path = os.path.join(OUT_DIR, f"review-{date}.csv")
    with open(path, "w", encoding="utf-8-sig", newline="") as f:
        w = csv.writer(f, lineterminator="\r\n")
        w.writerow(COLUMNS)
        for *_, row in rows:
            w.writerow(row)
    guide = os.path.join(OUT_DIR, f"review-{date}-ANLEITUNG.md")
    with open(guide, "w", encoding="utf-8", newline="\n") as f:
        f.write(ANLEITUNG.replace("{date}", date))
    n_prop = sum(1 for r in rows if r[1] == 0)
    print(f"export: {len(rows)} rows ({n_prop} proposed, {len(rows) - n_prop} confirmed)")
    print(f"  {os.path.relpath(path)}\n  {os.path.relpath(guide)}")
    return path


def decode_returned(raw):
    for enc in ("utf-8-sig", "cp1252"):
        try:
            return raw.decode(enc), enc
        except UnicodeDecodeError:
            continue
    raise SystemExit("import: file is neither UTF-8 nor Windows-1252, rejected")


def import_file(path, reviewer, date, dry_run):
    with open(path, "rb") as f:
        text, enc = decode_returned(f.read())
    if "�" in text:
        raise SystemExit("import: file contains the replacement character '�' (umlauts were lost "
                         "when saving), rejected. Ask the reviewer to save as 'CSV UTF-8'.")
    first = text.splitlines()[0] if text else ""
    delim = ";" if first.count(";") > first.count(",") else ","
    table = list(csv.reader(io.StringIO(text), delimiter=delim))
    if not table:
        raise SystemExit("import: empty file")
    head = [h.strip() for h in table[0]]
    def col(name_start):
        for i, h in enumerate(head):
            if h.lower().startswith(name_start.lower()):
                return i
        raise SystemExit(f"import: column '{name_start}' missing (header: {head})")
    i_key, i_de, i_ok, i_fix, i_comment = col("Key"), col("Deutsch"), col("OK?"), col("Korrektur"), col("Kommentar")

    header, strings = load_strings()
    by_key = {r[0]: r for r in strings}
    status, order = load_status()
    confirmed, corrected, errors, untouched = [], [], [], 0

    for n, r in enumerate(table[1:], start=2):
        r = r + [""] * (len(head) - len(r))
        key = r[i_key].strip()
        if not key:
            continue
        ok = r[i_ok].strip().lower()
        fix = r[i_fix].strip()
        comment = r[i_comment].strip()
        if key not in by_key:
            errors.append(f"line {n}: unknown key '{key}'")
            continue
        src = by_key[key]
        en, de_now = src[2], src[3]
        if not ok and not fix:
            untouched += 1
            continue
        if ok not in ("", "ja", "j", "yes", "nein", "n", "no"):
            errors.append(f"line {n} {key}: OK? must be 'ja' or 'nein', got '{r[i_ok]}'")
            continue
        if ok in ("nein", "n", "no") and not fix:
            errors.append(f"line {n} {key}: 'nein' without Korrekturvorschlag, left as is"
                          + (f" (Kommentar: {comment})" if comment else ""))
            continue
        new_de = fix if fix else de_now
        if sorted(PLACEHOLDER.findall(new_de)) != sorted(PLACEHOLDER.findall(en)):
            errors.append(f"line {n} {key}: placeholders {sorted(PLACEHOLDER.findall(new_de))} "
                          f"differ from EN {sorted(PLACEHOLDER.findall(en))}, status unchanged")
            continue
        if sorted(TAG.findall(new_de)) != sorted(TAG.findall(en)):
            errors.append(f"line {n} {key}: formatting tags {sorted(TAG.findall(new_de))} "
                          f"differ from EN {sorted(TAG.findall(en))}, status unchanged")
            continue
        note = comment
        if fix and fix != de_now:
            src[3] = fix
            note = "korrigiert durch Reviewer" + (f"; {comment}" if comment else "")
            corrected.append(key)
        else:
            confirmed.append(key)
        if key not in status:
            order.append(key)
        status[key] = [key, new_de, "confirmed", reviewer, date, note]

    print(f"import ({enc}, separator '{delim}'): {len(confirmed)} confirmed, {len(corrected)} corrected, "
          f"{untouched} not answered, {len(errors)} rejected")
    for e in errors:
        print("  ERROR " + e)
    if corrected:
        print("  corrected (run through german-reviewer, then LocalizationBinder.ImportCsv() in Unity): "
              + ", ".join(corrected))
    if dry_run:
        print("  dry run: nothing written")
    else:
        if corrected:
            write_csv(STRINGS, [header] + strings)
        write_csv(STATUS, [STATUS_HEADER] + [status[k] for k in order])
    return 1 if errors else 0


def write_csv(path, rows):
    """Rewrite only the rows whose values changed (or are new); every other row keeps its exact bytes,
    including its hand-made quoting, so the git diff shows just the reviewed rows."""
    with open(path, "rb") as f:
        raw = f.read()
    bom = raw.startswith(b"\xef\xbb\xbf")
    text = raw.decode("utf-8-sig")
    eol = "\r\n" if "\r\n" in text else "\n"
    lines = re.findall(r"[^\n]*\n|[^\n]+$", text)  # split on \n only (str.splitlines also splits on U+2028 etc.)
    consumed = []

    def feed():
        for line in lines:
            consumed.append(line)
            yield line

    old = []  # (key, values, raw text)
    reader = csv.reader(feed())
    for values in reader:
        old.append((values[0] if values else None, values, "".join(consumed)))
        consumed.clear()

    def fmt(values):
        buf = io.StringIO()
        csv.writer(buf, lineterminator=eol).writerow(values)
        return buf.getvalue()

    new_by_key = {r[0]: r for r in rows[1:]}
    out, seen = [], set()
    for i, (key, values, raw_row) in enumerate(old):
        if i == 0 or not key or key not in new_by_key:
            out.append(raw_row)
            continue
        seen.add(key)
        new = new_by_key[key]
        same = [v for v in values] + [""] * (len(new) - len(values)) == list(new)
        out.append(raw_row if same else fmt(new))
    for r in rows[1:]:
        if r[0] not in seen:
            if out and not out[-1].endswith(("\n", "\r")):
                out[-1] += eol
            out.append(fmt(r))
    with open(path, "w", encoding="utf-8-sig" if bom else "utf-8", newline="") as f:
        f.write("".join(out))


ANLEITUNG = """# Prüfung der deutschen Texte – Anleitung ({date})

Vielen Dank, dass Sie die Texte unserer VR-Schulung „Kommissionieren“ prüfen. Die Schulung ist für
Mitarbeiterinnen und Mitarbeiter im Lager gedacht, deren Muttersprache Deutsch ist. Die englischen Texte
sind nur für das Entwicklerteam da.

## Datei

`review-{date}.csv` öffnen Sie mit Excel (Doppelklick). Jede Zeile ist ein Text aus der App.
Die Zeilen sind nach Lektionen sortiert, in der Reihenfolge, in der die Teilnehmenden sie sehen;
noch nicht geprüfte Texte (`proposed`) stehen jeweils oben.

| Spalte | Bedeutung |
|---|---|
| Key | interner Name des Textes – bitte nicht ändern |
| Kontext | wo der Text erscheint und wie viel Platz er hat (auf Englisch) |
| English | englische Fassung, nur zur Erklärung |
| Deutsch | der Text, den die Teilnehmenden sehen |
| Status | `proposed` = noch nicht geprüft, `confirmed` = schon geprüft |
| OK? (ja/nein) | bitte ausfüllen |
| Korrekturvorschlag | Ihr besserer Text, nur wenn „nein“ |
| Kommentar | alles, was Sie uns sagen möchten |

## Was Sie prüfen

1. **Fachbegriffe:** Sagt man das so im Lager? (z. B. Lagerbehälter, Auftragsbehälter, Karton, Tüte,
   lose Teile, kommissionieren)
2. **Natürlich:** Klingt der Satz wie von einer Trainerin oder einem Trainer im Lager, nicht wie übersetzt?
3. **Verständlich:** Versteht eine Lagermitarbeiterin oder ein Lagermitarbeiter ohne Vorwissen,
   was zu tun ist?
4. **Anrede:** immer „Sie“.
5. **Rechtschreibung und Grammatik.**

## Bitte nicht ändern

- **Bezeichnungen aus dem LVS** bleiben genau wie im echten System, auch wenn sie ungewöhnlich wirken:
  `Mat.-Nr`, `Bez.`, `Verp.-Einh.`, `Soll`, `Ist`, `Rest`, `Empf Kz`, `F6 Bestätigen`, `ST`.
- **Platzhalter in geschweiften Klammern**, z. B. `{position}` oder `{cartons}`: Die App setzt dort eine
  Zahl ein. Sie dürfen sie im Satz verschieben, aber nicht löschen oder umbenennen.
- **Zeichen in spitzen Klammern**, z. B. `<b>` … `</b>` (fett) oder `<nobr>` … `</nobr>`
  (nicht umbrechen): bitte stehen lassen.
- Die Zeichen `\\n` bedeuten einen Zeilenumbruch.

## So füllen Sie aus

- Text ist gut: **OK?** = `ja`.
- Text soll anders sein: **OK?** = `nein` und Ihr Text in **Korrekturvorschlag**.
- Unsicher oder Frage: **Kommentar** ausfüllen, **OK?** leer lassen.
- Zeilen, die Sie nicht geprüft haben, lassen Sie einfach leer.
- Texte sollen kurz bleiben: Die Felder in der Brille sind klein (siehe „max …“ im Kontext).

## Speichern

In Excel: **Datei → Speichern unter → „CSV UTF-8 (durch Trennzeichen getrennt)“**.
Bitte nicht „CSV (Trennzeichen-getrennt)“ wählen, sonst gehen die Umlaute verloren.
Schicken Sie die Datei danach an uns zurück.
"""


def main():
    p = argparse.ArgumentParser(description=__doc__, formatter_class=argparse.RawDescriptionHelpFormatter)
    sub = p.add_subparsers(dest="cmd", required=True)
    e = sub.add_parser("export")
    e.add_argument("--date", default=datetime.date.today().isoformat())
    i = sub.add_parser("import")
    i.add_argument("file")
    i.add_argument("--reviewer", required=True)
    i.add_argument("--date", default=datetime.date.today().isoformat())
    i.add_argument("--dry-run", action="store_true")
    a = p.parse_args()
    if a.cmd == "export":
        export(a.date)
        return 0
    return import_file(a.file, a.reviewer, a.date, a.dry_run)


if __name__ == "__main__":
    sys.exit(main())
