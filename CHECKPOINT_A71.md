# Checkpoint A71 — physical production logistics

Дата: 2026-09-15

## Готово

- `ProductionSupply` резервирует сырьё в конкретном ближайшем operational stockpile.
- Профильный рабочий идёт к этому stockpile, забирает ресурс и доставляет его в input storage workstation.
- Производство резервирует room output storage до начала цикла.
- `RoomOutputHaul` резервирует место в конкретном stockpile и переносит туда до 6 единиц.
- Отмена освобождает source pickup, room pickup и destination space reservations.
- Save v35 хранит physical endpoint в существующем `DestinationRoomId`; старые незавершённые jobs без endpoint снимаются.
- Размер settlement map сохранён: 768×768.

## Проверка

- `python3 Tools/verify_port.py`.
- 123 активных C#-файла, 36 359 строк.
- Godot 4 .NET и .NET SDK в среде отсутствуют; сборка и игровой smoke test не выполнялись.

## Дальше

A72: физическое размещение loose resources в конкретных stockpile slots вместо промежуточного общего счётчика.
