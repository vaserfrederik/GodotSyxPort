# Checkpoint A79 — source-order FurnisherStat evaluation

Дата: 2026-09-16

## Готово

- Raw furniture stats суммируются как `ITEMS.STATS × multiplierStats` по размещённым item groups.
- `FurnisherStatRelative` использует `clamp(mod × value / denominator, 0, 1)`.
- `FurnisherStatEfficiency` использует `clamp(0.5 + mod × 0.5 × value / workers, 0, 1)`.
- `FurnisherStatEmployeesR` выводит штат из базового service stat.
- Зависимые stats вычисляются в порядке их объявления Java.
- Сохранены исходные коэффициенты bath `1.5`, lavatory `1/8` и nursery `0.2`.
- Итоговые stats подключены к service capacity, школам, inn/resthome, law facilities и production.
- Панель draft-комнаты показывает те же преобразованные значения.
- Save-миграции не добавлялись; settlement map остаётся строго 768×768.

## Проверка

- `python3 Tools/verify_port.py` → `PORT_STATIC_VERIFICATION_OK`.
- 124 активных C#-файла, 37 037 строк.
- Godot 4 .NET/.NET SDK отсутствуют, поэтому runtime smoke test не выполнялся.

## Дальше

A80: `FurnisherStat.min` и `FurnisherItemGroup.min/max` в placement validation.
