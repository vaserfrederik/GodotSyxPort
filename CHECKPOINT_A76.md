# Checkpoint A76 — exact furniture cost/stat multipliers

Дата: 2026-09-15

## Готово

- Экстрактор сохраняет отдельные `FurnisherItem.multiplierCosts` и `multiplierStats`.
- Множители хранятся как `double`; дробные значения вроде `1.5` больше не обрезаются.
- Каждый размещённый layout накапливает отдельные cost totals и stat totals по своей item group.
- Construction получает только cost totals; room capacity/quality/service consumers получают stat totals.
- Цена повторяет `ConstructionInstance.finish`: `ceil(area costs + item costs)` выполняется один раз.
- Проверены исходные layouts, где множители различаются, включая stockpile и import depot.
- Текущий save хранит точные stat totals как `double`; миграции старых save не добавлялись.
- Settlement map остаётся строго 768×768.

## Проверка

- `python3 Tools/verify_port.py` → `PORT_STATIC_VERIFICATION_OK`.
- 659 исходных layouts, 123 активных C#-файла, 36 738 строк.
- Godot 4 .NET/.NET SDK отсутствуют, поэтому runtime smoke test не выполнялся.

## Дальше

A77: перенести `FurnisherItem.brokenResourceAmount`, поломку и ремонт отдельных furniture items.
