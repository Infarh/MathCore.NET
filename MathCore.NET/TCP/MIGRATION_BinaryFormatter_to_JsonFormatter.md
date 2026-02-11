# Миграция с BinaryFormatter на JsonFormatter

## Обзор изменений

Заменили устаревший и небезопасный `BinaryFormatter` на новую реализацию `JsonFormatter`, которая:

- ✅ Безопаснее (не выполняет код во время десериализации)
- ✅ Совместима с .NET Standard 2.0
- ✅ Поддерживает все основные типы данных
- ✅ Легко читается и отлаживается (текстовый формат)

## Технические детали

### JsonFormatter

Новый класс `JsonFormatter` реализует интерфейс `IFormatter` и использует:

- **Формат сохранения**: Структурированный двоичный формат с метаинформацией о типе
  - 4 байта: длина имени типа
  - N байт: AssemblyQualifiedName типа
  - 4 байта: длина данных
  - M байт: JSON-представление объекта

- **Поддерживаемые типы**:
  - Примитивные типы (int, double, bool, string и т.д.)
  - DateTime
  - Enum
  - Пользовательские объекты (через публичные свойства)
  - Массивы и коллекции (базовая поддержка)

### Использование

```csharp
// Как было:
protected IFormatter _DataFormatter = new BinaryFormatter();

// Теперь:
protected IFormatter _DataFormatter = new JsonFormatter();
```

Код работает прозрачно - интерфейс остался неизменным.

## Преимущества

1. **Безопасность**: JSON не содержит исполняемого кода
2. **Совместимость**: Работает с .NET Standard 2.0+
3. **Отладка**: Текстовый формат можно читать в Binary Inspector
4. **Расширяемость**: Легко добавить поддержку новых типов

## Миграция с BinaryFormatter

Если у вас есть старые сохраненные данные в BinaryFormatter:

1. Создайте конвертер для миграции:
```csharp
var oldFormatter = new BinaryFormatter();
var newFormatter = new JsonFormatter();

using (var oldStream = File.OpenRead("old_data.bin"))
{
    var obj = oldFormatter.Deserialize(oldStream);
    
    using (var newStream = File.Create("new_data.bin"))
    {
        newFormatter.Serialize(newStream, obj);
    }
}
```

## Ограничения

- Сложные объекты с приватными полями требуют дополнительной настройки
- Циклические ссылки не поддерживаются
- Некоторые типы (например, делегаты) требуют специальной обработки

## Файлы, затронутые изменением

- `MathCore.NET/TCP/JsonFormatter.cs` — новая реализация (создан)
- `MathCore.NET/TCP/Client.cs` — обновлена инициализация `_DataFormatter`
