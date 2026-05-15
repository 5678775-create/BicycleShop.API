# Практична робота №2

## Складні бізнес-правила: workflow станів, транзакції, ціноутворення та інвентар

Проєкт BicycleShop API розширено бізнес-логікою для оформлення замовлень, резервування товарів, керування складом, промо-акцій і прозорого розрахунку ціни.

## Що реалізовано

- Створення `Order` через `POST /api/orders`.
- Збереження `OrderItems` разом із замовленням.
- Перевірка наявності товару перед створенням замовлення.
- Окрема сутність `Inventory` для складських залишків.
- `InventoryReservation` для резервування товарів під замовлення.
- Звільнення резерву при скасуванні замовлення.
- Списання резерву зі складу при переході замовлення у `Shipped`.
- Workflow статусів `Pending`, `Confirmed`, `Shipped`, `Delivered`, `Cancelled`.
- Транзакційне створення замовлення разом з позиціями, знижками, резервами і оновленням складу.
- Customer loyalty tiers: `Bronze`, `Silver`, `Gold`.
- Tier-based discounts.
- Bulk discounts.
- Промо-акції типів `TimeBased` і `CategoryBased`.
- `AppliedPromotion` для прозорої відповіді API про застосовані промо.
- Unit-тести для `PricingService`.
- Unit-тести для `OrderWorkflowService`.

## Основні API endpoint-и

- `POST /api/orders` - створення замовлення, розрахунок ціни, резервування inventory.
- `GET /api/orders/{id}` - отримання замовлення з позиціями, знижками і резервами.
- `PATCH /api/orders/{id}/status` - зміна статусу з перевіркою workflow.
- `POST /api/orders/{id}/cancel` - скасування замовлення і звільнення резерву.
- `GET /api/promotions` - список усіх промо-акцій.
- `GET /api/promotions/active` - активні промо-акції.
- `POST /api/promotions` - створення промо-акції.
- `GET /api/inventory` - список складських записів.
- `GET /api/inventory/{productId}` - складський запис для товару.
- `PATCH /api/inventory/{productId}` - оновлення доступної кількості товару.

Усі endpoint-и реалізовані через ASP.NET Core controllers і доступні для Swagger/OpenAPI.

## Workflow статусів

Дозволені переходи:

- `Pending -> Confirmed`
- `Confirmed -> Shipped`
- `Shipped -> Delivered`
- `Pending -> Cancelled`
- `Confirmed -> Cancelled`

Заборонені переходи:

- `Pending -> Shipped`
- `Pending -> Delivered`
- `Confirmed -> Delivered`
- `Shipped -> Pending`
- `Shipped -> Cancelled`
- `Delivered -> Pending`
- `Delivered -> Confirmed`
- `Delivered -> Shipped`
- `Delivered -> Cancelled`
- `Cancelled -> Pending`
- `Cancelled -> Confirmed`
- `Cancelled -> Shipped`
- `Cancelled -> Delivered`

Якщо перехід заборонений, `OrderWorkflowService` кидає `InvalidOrderStatusTransitionException`, а API повертає `409 Conflict`.

## Pricing rules

Порядок розрахунку:

1. `Subtotal = sum(UnitPrice * Quantity)`.
2. Loyalty discount.
3. Bulk discount.
4. Promotion discount.
5. `FinalAmount = Subtotal - TotalDiscountAmount`.

Loyalty discount:

- `Bronze` - 2%.
- `Silver` - 5%.
- `Gold` - 10%.

Bulk discount:

- якщо загальна кількість товарів у замовленні `>= 3`, застосовується 5%;
- якщо subtotal `>= 50000`, застосовується 7%;
- якщо виконуються обидва правила, застосовується більша bulk-знижка.

Promotions:

- `TimeBased` застосовується, якщо поточна дата між `ActiveFrom` і `ActiveTo`.
- `CategoryBased` застосовується тільки до товарів відповідного `CatalogId`.

`FinalAmount` не може бути меншим за 0. У відповіді API показуються `Subtotal`, кожен тип знижки, `TotalDiscountAmount`, `TotalAmount` і список `AppliedPromotions`.

## Транзакційність

Створення замовлення виконується в одній EF Core transaction:

- створюється `Order`;
- створюються `OrderItems`;
- зберігаються `AppliedPromotions`;
- створюються `InventoryReservations`;
- оновлюється `Inventory.QuantityReserved`.

Якщо на будь-якому етапі виникає помилка, transaction виконує rollback. Це не залишає в базі “напівзамовлення” без позицій, резервів або коректних складських змін.

## Резервування

- При створенні замовлення система перевіряє `QuantityAvailable - QuantityReserved`.
- Якщо товар доступний, створюється `InventoryReservation`, а `QuantityReserved` збільшується.
- При переході у `Cancelled` активні резерви звільняються, `QuantityReserved` зменшується, reservation позначається як released.
- При переході у `Shipped` активні резерви списуються зі складу: `QuantityAvailable` і `QuantityReserved` зменшуються, reservation позначається як committed.
- Повторне release або commit уже обробленого reservation не виконується, бо сервіс працює тільки з активними резервами.

## Error handling

Додано централізований middleware для API errors:

- validation errors - `400 Bad Request`;
- `NotFoundException` - `404 Not Found`;
- `InsufficientInventoryException` - `409 Conflict`;
- `InvalidOrderStatusTransitionException` - `409 Conflict`;
- `BusinessRuleValidationException` - `409 Conflict`;
- unexpected errors - `500 Internal Server Error`.

Відповідь має формат `ApiErrorResponse` з полями `message`, `details`, `statusCode`.

## EF Core migration

Створена migration:

- `AddOrderWorkflowPricingInventory`

Вона додає:

- `OrderItems`;
- `Inventories`;
- `InventoryReservations`;
- `Promotions`;
- `AppliedPromotions`;
- додаткові поля для order totals and discounts;
- `Customer.LoyaltyTier`;
- precision `decimal(18,2)` для грошових полів;
- seed data для customers, catalogs, products, inventory і promotions.

`database update` не виконувався.

## Unit-тести

Додано тестовий проєкт `BicycleShop.Tests`.

Покрито:

- Bronze/Silver/Gold loyalty discount.
- Bulk discount за кількістю товарів.
- Bulk discount за subtotal `>= 50000`.
- TimeBased promotion в активному періоді.
- TimeBased promotion поза активним періодом.
- CategoryBased promotion для правильного catalog.
- CategoryBased promotion для іншого catalog.
- Захист від від’ємного final amount.
- Дозволені workflow переходи.
- Заборонені workflow переходи.
- Текст помилки для invalid status transition.

## Code smells і больові точки

1. Fat Service. `OrderService` координує створення замовлення, pricing, inventory і workflow. Для лабораторної це прийнятно, але з ростом системи сервіс може стати занадто великим.

2. Anemic Domain Model. Сутності переважно містять дані, а бізнес-логіка винесена в application services. Це типово для layered architecture, але не ідеально для DDD.

3. Primitive Obsession. `Money`, `Quantity`, `DiscountPercent` представлені як `decimal` і `int`, а не як value objects з власними інваріантами.

4. Duplicated Business Rules Risk. Правила статусів, інвентарю або ціноутворення можуть почати дублюватися в controllers/services, якщо систему розширювати без окремих policy/strategy об’єктів.

5. Low Testability Risk. Логіка, яка напряму залежить від `DbContext`, складніше тестується unit-тестами. Для таких сценаріїв краще додавати integration tests або виносити правила у чисті доменні сервіси.
