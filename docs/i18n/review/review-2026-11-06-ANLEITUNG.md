# Prüfung der deutschen Texte – Anleitung (2026-11-06)

Vielen Dank, dass Sie die Texte unserer VR-Schulung „Kommissionieren“ prüfen. Die Schulung ist für
Mitarbeiterinnen und Mitarbeiter im Lager gedacht, deren Muttersprache Deutsch ist. Die englischen Texte
sind nur für das Entwicklerteam da.

## Datei

`review-2026-11-06.csv` öffnen Sie mit Excel (Doppelklick). Jede Zeile ist ein Text aus der App.
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
- Die Zeichen `\n` bedeuten einen Zeilenumbruch.

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
