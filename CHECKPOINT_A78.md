# Checkpoint A78 — atomic broken furniture footprint

Дата: 2026-09-16

## Готово

- Каждая построенная мебель сохраняет точный атомарный footprint и anchor.
- Поломка любой клетки помечает broken весь footprint.
- Blocker-роли сохраняются, но workstation и storage на broken-клетках отключаются.
- Число готовой мебели уменьшается, поэтому комната выходит из `Operational`.
- Существующий readiness-контур отменяет production/supply jobs остановленной комнаты.
- Broken footprint получает отдельный maintenance-job с физическим repair-ресурсом.
- После завершения job весь footprint восстанавливается одновременно.
- Footprint membership, anchor и broken-state сохраняются в текущем save.
- Миграции старых save не добавлялись; settlement map остаётся строго 768×768.

## Проверка

- `python3 Tools/verify_port.py` → `PORT_STATIC_VERIFICATION_OK`.
- 659 исходных layouts, 123 активных C#-файла, 36 943 строки.
- Godot 4 .NET/.NET SDK отсутствуют, поэтому runtime smoke test не выполнялся.

## Дальше

A79: точные преобразования `FurnisherStat` и подключение efficiency/relative stats к мирным комнатам.
