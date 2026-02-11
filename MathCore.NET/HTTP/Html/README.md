# MathCore.NET HTTP HTML Builder

Библиотека для удобного и быстрого создания HTML-разметки на C#. Она предоставляет типизированный API, который позволяет программно конструировать HTML-документы с использованием стандартных классов, представляющих HTML-элементы.

## Архитектура

### Базовые классы

#### `HElementBase`
Абстрактный базовый класс для всех HTML-элементов. Определяет следующие основные методы:
- `InnerText()` — возвращает внутренний текст элемента
- `ToString(int level)` — преобразует элемент в HTML-строку с указанным уровнем отступа

#### `HElement`
Конкретный класс для создания HTML-элементов общего назначения. Поддерживает:
- **Вложенные элементы** — хранятся в коллекции `Elements`
- **Атрибуты** — хранятся в коллекции `Attributes`
- **Гибкое заполнение** — метод `Add()` принимает элементы, атрибуты, строки и объекты
- **Форматирование** — автоматически форматирует вывод с отступами

Важные свойства:
- `Name` — имя HTML-тега
- `AlwaysOpen` — если `true`, тег не закрывается (например, `<body>...</body>` вместо `<body/>`)
- `OnlyOpen` — если `true`, выводится только открывающий тег

#### `TypedElement`
Абстрактный класс для типизированных HTML-элементов с предопределённым именем. Наследуется от `HElement` и переопределяет свойство `Name` как только для чтения, предотвращая изменение имени элемента.

#### `HAttribute`
Представляет HTML-атрибут элемента. Содержит имя и значение атрибута.

Специализированные атрибуты:
- `ClassAttribute` — атрибут `class`
- `IdAttribute` — атрибут `id`

## Типизированные элементы

Библиотека содержит типизированные классы для большинства стандартных HTML-элементов:

**Структурные элементы:**
- `Body` — элемент `<body>`
- `Head` — элемент `<head>`
- `Header` — элемент `<header>`
- `Footer` — элемент `<footer>`
- `Article` — элемент `<article>`
- `Section` — элемент `<section>`
- `Div` — элемент `<div>`
- `Title` — элемент `<title>`

**Текст и форматирование:**
- `P` — элемент `<p>` (абзац)
- `Text` — элемент для обычного текста
- `H`, `H1`, `H2`, `H3`, `H4` — заголовки различных уровней

**Списки:**
- `NumberedList` — элемент `<ol>` (нумерованный список)
- `MarkedList` — элемент `<ul>` (ненумерованный список)
- `MenuList` — элемент `<menu>`
- `ListItem` — элемент `<li>` (элемент списка)
- `DataList` — элемент `<dd>` (список определений)
- `DataListItem` — элемент с парой `<dd>` и `<dt>`

**Таблицы:**
- `Table` — элемент `<table>`
- `TableHeader` — элемент `<thead>`
- `TableBody` — элемент `<tbody>`

**Ссылки и ресурсы:**
- `Href` — элемент `<a>` (гиперссылка)
- `Link` — элемент `<link>` (ссылка на ресурс)
- `Script` — элемент `<script>`

**Полная страница:**
- `Page` — представляет полную HTML-страницу с `<html>`, `<head>` и `<body>`

## Примеры использования

### Простой текст и элементы

```csharp
// Создание обычного текста
var text = new Text("Hello, World!");

// Или через неявное преобразование
HElementBase simpleText = "Hello, World!";
```

### Заголовки

```csharp
// Заголовок 1-го уровня
var h1 = new H1(new Text("Добро пожаловать"));

// Заголовок с множеством элементов
var h2 = new H2("Заголовок", new Text(" второго уровня"));

// Создание через базовый класс H
var h3 = new H(3, "Заголовок третьего уровня");
```

### Абзацы и контейнеры

```csharp
// Простой абзац
var paragraph = new P("Это текст абзаца");

// Абзац с несколькими элементами
var complexP = new P(
    "Первая часть текста",
    new Href("https://example.com", "ссылка"),
    " и остаток текста"
);

// Контейнер div
var div = new Div(
    new H2("Заголовок раздела"),
    new P("Описание раздела")
);
```

### Атрибуты

```csharp
// Добавление атрибутов к элементу
var div = new Div();
div.Add(new ClassAttribute("container"));
div.Add(new IdAttribute("main-content"));

// Можно также добавить через метод Add с обычными атрибутами
var link = new Href("https://example.com", "Нажми");
link.Add(new HAttribute("target", "_blank"));
```

### Списки

```csharp
// Нумерованный список
var numberedList = new NumberedList(
    new ListItem("Первый пункт"),
    new ListItem("Второй пункт"),
    new ListItem("Третий пункт")
);

// Ненумерованный список
var markedList = new MarkedList(
    new ListItem("Пункт А"),
    new ListItem("Пункт Б"),
    new ListItem("Пункт В")
);
```

### Таблицы

```csharp
// Создание таблицы
var table = new Table();

// Установка заголовка
table.Header = new TableHeader(
    new HElement("tr",
        new HElement("th", "Имя"),
        new HElement("th", "Возраст")
    )
);

// Установка тела таблицы
table.Body = new TableBody(
    new HElement("tr",
        new HElement("td", "Иван"),
        new HElement("td", "30")
    ),
    new HElement("tr",
        new HElement("td", "Мария"),
        new HElement("td", "28")
    )
);
```

### Полная HTML-страница

```csharp
// Создание полной страницы
var page = new Page();

// Установка заголовка страницы
page.Title = "Моя первая страница";

// Добавление контента в head
page.Head.Add(new Link("stylesheet", "https://example.com/style.css"));

// Добавление контента в body
page.Body.Add(
    new Header(
        new H1("Заголовок сайта")
    ),
    new Section(
        new H2("О проекте"),
        new P("Описание проекта...")
    ),
    new Footer(
        new P("© 2024 Все права защищены")
    )
);

// Получение HTML-кода
string htmlOutput = page.ToString();
```

### Более сложный пример: личная страница

```csharp
var page = new Page();
page.Title = "Моя личная страница";

// Добавление стилей в head
page.Head.Add(new Link("stylesheet", "/styles.css"));

// Построение body
page.Body.Add(
    new Header(
        new H1("Иван Иванов"),
        new P("Разработчик программного обеспечения")
    ),
    
    new Section(
        new H2("Об авторе"),
        new P("Я — опытный разработчик с 5 годами опыта..."),
        new P("Мои навыки:")
    ).Add(new ClassAttribute("about")),
    
    new Section(
        new H2("Проекты"),
        new MarkedList(
            new ListItem(
                new Href("https://github.com/example/project1", "Проект 1")
            ),
            new ListItem(
                new Href("https://github.com/example/project2", "Проект 2")
            )
        )
    ).Add(new ClassAttribute("projects")),
    
    new Footer(
        new P("Свяжитесь со мной: "),
        new Href("mailto:ivan@example.com", "ivan@example.com")
    )
);

var html = page.ToString();
```

### Использование HElement для создания произвольных элементов

```csharp
// Если нужен элемент, который не имеет типизированного класса
var customElement = new HElement("article",
    new HElement("header",
        new HElement("h1", "Статья")
    ),
    new HElement("main",
        new HElement("p", "Содержимое статьи")
    )
);

// Добавление атрибутов
customElement.Add(new HAttribute("data-id", "12345"));
```

## Форматирование и отступы

По умолчанию HTML-код форматируется с отступами. Строка отступа определяется статическим свойством:

```csharp
// По умолчанию это 4 пробела
HElementBase.IdentPattern = "    ";

// Можно изменить на табуляцию
HElementBase.IdentPattern = "\t";

// Или на 2 пробела
HElementBase.IdentPattern = "  ";
```

## Получение текста

Для получения только внутреннего текста элемента без HTML-тегов используйте метод `InnerText()`:

```csharp
var p = new P("Это текст");
string textOnly = p.InnerText(); // Выведет: "Это текст"
```

Для получения полного HTML-кода элемента с тегами:

```csharp
var p = new P("Это текст");
string html = p.ToString(); // Выведет: "<p>Это текст</p>"
```

## Советы по использованию

1. **Используйте типизированные классы** для стандартных HTML-элементов — это обеспечивает типобезопасность и лучшую читаемость кода.

2. **Преобразование строк в Text** происходит автоматически через неявный оператор преобразования.

3. **Добавление элементов гибко** — метод `Add()` принимает элементы, атрибуты, строки и объекты, автоматически преобразуя их в соответствующие типы.

4. **Форматирование сложных структур** — библиотека автоматически добавляет переносы строк и отступы для улучшения читаемости HTML.

5. **Установка свойств AlwaysOpen и OnlyOpen** полезна для элементов, которые должны оставаться открытыми (например, `<body>`, `<head>`) или иметь только открывающий тег (например, `<link>`, `<img>`).

## Лицензия

Часть библиотеки MathCore.NET. Смотрите основной репозиторий для информации о лицензии.
