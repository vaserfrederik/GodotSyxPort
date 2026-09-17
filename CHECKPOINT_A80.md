# Checkpoint A80 — furnisher minimum and group validation

Дата: 2026-09-16

## Готово

- Добавлен extractor для Java `flush(min,max,rotations)` и `FurnisherStat.min`.
- Извлечено 109 записей `FurnisherItemGroup`.
- Group count считается по master-items, а не по cost/stat multiplier.
- Каждая группа проверяет исходные minimum и maximum.
- После group constraints проверяются минимумы преобразованных `FurnisherStat`.
- Порядок соответствует `UtilPlacability.placable`.
- Невалидный draft не создаёт room и construction jobs.
- Карта остаётся строго 768×768; save-миграции не добавлялись.

## Проверка

- `python3 Tools/verify_port.py` → `PORT_STATIC_VERIFICATION_OK`.
- 125 активных C#-файлов, 37 107 строк.
- Godot 4 .NET/.NET SDK отсутствуют, поэтому runtime smoke test не выполнялся.

## Дальше

A81: допустимые rotations каждой furniture group из Java `flush`.
