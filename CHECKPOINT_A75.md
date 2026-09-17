# Checkpoint A75 — exact furniture work/storage roles

Дата: 2026-09-15

## Готово

- Экстрактор layouts читает функциональные `setData`-маркеры исходных `FurnisherItemTile`.
- `B_WORK` и job-маркеры становятся точными workstation-клетками.
- `B_STORAGE` и crate-маркеры становятся точными storage-клетками.
- Work/storage роли вращаются вместе с footprint и проходят через placement и атомарный construction job.
- После строительства роли записываются в плотный tile runtime отдельными битами.
- Industry worker maximum и production jobs считают только построенные workstation-клетки.
- Внутреннее хранилище использует точные storage-клетки; fallback остаётся для исходных layouts без storage-маркеров.
- Текущий save хранит незавершённые furniture work/storage-роли; миграции старых save не добавлялись.
- Settlement map остаётся строго 768×768.

## Проверка

- `python3 Tools/verify_port.py` → `PORT_STATIC_VERIFICATION_OK`.
- 659 исходных layouts, 123 активных C#-файла, 36 712 строк.
- Godot 4 .NET/.NET SDK отсутствуют, поэтому runtime smoke test не выполнялся.

## Дальше

A76: перенести исходные furniture item stats/multiplier в стоимость строительства и room efficiency.
