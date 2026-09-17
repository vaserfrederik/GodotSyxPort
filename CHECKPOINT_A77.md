# Checkpoint A77 — resource-aware furniture maintenance

Дата: 2026-09-16

## Готово

- Реализована формула `FurnisherItem.brokenResourceAmount`: `ceil(groupCost × multiplierCosts / itemArea)`.
- Для каждой клетки размещённого furniture footprint хранится точный repair-профиль ресурсов.
- Room maintenance использует фактические construction-resource totals вместо `Stone + Wood × 2`.
- Вероятность material-job равна доле ресурсного слагаемого в полной скорости `MRoom`.
- Вид ресурса выбирается пропорционально исходным resource amounts.
- Материальный maintenance-job резервирует, забирает и доставляет одну физическую единицу ресурса.
- Текущий save хранит room maintenance totals и per-cell furniture repair map.
- Миграции старых save не добавлялись; settlement map остаётся строго 768×768.

## Проверка

- `python3 Tools/verify_port.py` → `PORT_STATIC_VERIFICATION_OK`.
- 659 исходных layouts, 123 активных C#-файла, 36 825 строк.
- Godot 4 .NET/.NET SDK отсутствуют, поэтому runtime smoke test не выполнялся.

## Дальше

A78: дискретное broken-состояние целого furniture footprint и отключение его функций до ремонта.
