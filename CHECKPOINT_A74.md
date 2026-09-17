# Checkpoint A74 — atomic multi-resource production

Дата: 2026-09-15

## Готово

- Все recipe inputs проверяются до изменения дробного production progress.
- Фактическое целое потребление прогнозируется через `IndustryRuntime.PreviewAdvance`.
- При нехватке любого input цикл не создаёт outputs и не меняет progress.
- Output storage резервируется заранее для каждого выхода рецепта.
- Multi-output recipes, включая pasture, используют отдельные resource reservations.
- Дополнительные outputs больше не обходят reservation через unreserved deposit.
- Избыток сверх зарезервированного базового выхода остаётся физическим loose cargo.
- Миграции старых save не добавлялись; settlement map остаётся 768×768.

## Проверка

- `python3 Tools/verify_port.py`.
- 123 активных C#-файла, 36 640 строк.
- Godot 4 .NET/.NET SDK отсутствуют, поэтому runtime smoke test не выполнялся.

## Дальше

A75: точное число workstation/worker по мебельным ролям, размерам layout и доступным сторонам.
