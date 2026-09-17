# Checkpoint A72 — physical loose-resource stockpiling

Дата: 2026-09-15

## Готово

- Обычный `Haul` резервирует место в конкретном operational stockpile.
- Job хранит точный `DestinationRoomId` и физически доставляет груз в этот runtime.
- Промежуточные `_storedUnits` и `_reservedStorageUnits` удалены.
- Заполненность UI вычисляется из фактического содержимого и reservations складов.
- Accounted loose и accounted cargo-in-transit исключены из stockpile reconcile до доставки.
- Текущий save хранит `OutputAlreadyAccounted`; миграции старых сохранений не добавлялись.
- Settlement map остаётся 768×768.

## Проверка

- `python3 Tools/verify_port.py`.
- 123 активных C#-файла, 36 390 строк.
- Godot 4 .NET/.NET SDK отсутствуют, поэтому runtime smoke test не выполнялся.

## Дальше

A73: resource filters, crate limits, priorities и разрешение приёма ресурсов stockpile.
