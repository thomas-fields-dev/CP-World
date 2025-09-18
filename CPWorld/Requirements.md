# Requirements
## Models(Code First)
- Order
- OrderId(PK, int)
- CustomerName(string, required, max length 100)
- OrderDate(DateTime)
- Items(ICollection<OrderItem>)
___
## OrderItem
- OrderItemId(PK, int)
- ProductName(string, required, max length 100)
- Quantity(int)
- Price(decimal)
- Navigation → OrderId(FK), Order
___
## DbContext
- Create AppDbContext with DbSet<Order> and DbSet<OrderItem>.
- Configure relationships so deleting an Order also deletes its Items (cascade).
___
## Controller — OrdersController
- Index() → list all orders with customer + order date (include items count).
- Details(int id) → show one order with all its items.- 
- Create() (GET) → return form.
- Create(Order order) (POST) → save new order with multiple items into EF.
___
## Views
- Index.cshtml → Table of Orders (Id, Customer, Date, ItemCount).
- Details.cshtml → Show Order + loop through Items.
- Create.cshtml → Form to add order + dynamically add at least 2 items.
___
## Extras
- Use EF migrations (Add-Migration, Update-Database).
- Use Include() when loading related data.
- Bonus: Add basic server-side validation with[Required], [StringLength], [Range].
___
## 👉 Deliverable: Write
- Models
- DbContext
- Controller (with EF)
- Views (Index, Details, Create)
___
### 🔥 Expanded Requirements for MVC + EF Interview Prep

# 1. Models(Domain + Validation + Relationships)
## Order
- Add[Required], [StringLength] validation attributes.
- Add computed property → TotalAmount (sum of item prices).
- Add status field: OrderStatus(enum: Pending, Shipped, Cancelled).

## OrderItem
- Validation: [Range(1, 100)] for Quantity.
- Remove raw ProductName + Price duplication, instead add:
- ItemId(FK to Item table).
- Navigation: Item(so OrderItem links to catalog items).

## Item
- Already exists in your schema(ItemId, ProductName, QuantityAvailable, Price).
- Bonus: Track IsActive so discontinued items don’t appear in dropdowns.
- 👉 Interview win: Show you understand normalization(don’t store product/price repeatedly in OrderItem → reference Item).
___
# 2. DbContext
## Add `DbSet<Item>` Configure
- Order → OrderItems relationship with cascade delete.
- OrderItem → Item(FK).
- Use Fluent API in OnModelCreating to configure relationships + constraints.
- (Shows you can go beyond data annotations.)
___
# 3. Controller Enhancements
## Index
- Add filtering(search orders by CustomerName or filter by OrderStatus).
- Add pagination(skip/take).

## Details
- Show computed TotalAmount.

## Create
- Populate a dropdown with Item list(from DB) to add to an order.
- Ensure stock check: don’t allow ordering more than QuantityAvailable.

## Edit
- Allow modifying Order (add/remove OrderItems).

## Delete
- Confirm cascade delete works.

### 👉 Interview win: If you mention async actions (await _context.Orders.Include(...).ToListAsync()) you’ll look extra sharp.**
___
# 4. Views
## Index.cshtml
- Add search box + filter dropdown for status.
- Show pagination links.

## Details.cshtml
- Show order details, items, total, and current stock left (join with Item table).

## Create.cshtml
- Dropdowns for products.
- Dynamic JavaScript to add/remove rows for items.
- Validation messages(asp-validation-for).

## Edit.cshtml
- Similar to create but pre-populated.
___
# 5. Extras(Interview Show-Offs)
## Validation
- Client + server side validation with ModelState.IsValid.

## Error handling
- Graceful handling when order not found(return NotFound()).

## Dependency Injection
- Abstract EF calls behind IOrderRepository.

## Services
- Business logic(like checking stock availability) in a service layer.

## AutoMapper
- Use DTOs/ViewModels to avoid exposing EF entities directly.

## Unit Tests
- Mock DbContext(with InMemory provider).
- Test controller actions.
___
# 6. Advanced Interview Topics
## Lazy vs Eager vs Explicit loading
- .Include() → eager loading.
- Lazy loading pitfalls (N+1).
- When to use.Load().

## Performance optimizations
- .AsNoTracking() for read-only queries.
- Pagination(skip/take)

## Security
- Prevent overposting(use[Bind] or ViewModels).
- Validate input before saving to DB.

## Architecture
- Move logic to Services/Repositories.
- Discuss Repository vs Unit of Work patterns.
---
# C# / .NET MVC + EF Advanced Requirements List

## Models / Data
- Build a generic repository that works for all entities (→ Generics, constraints)  
- Add a search API where filters can be combined dynamically (→ Expression trees, delegates)  
- Make `Order` editing concurrency-safe so two users editing won’t overwrite each other (→ RowVersion, concurrency tokens)  
- Introduce soft deletes for `Item` so deleted items don’t vanish from history (→ EF global query filters)  
- Log every entity’s last updated timestamp automatically (→ EF shadow properties)  

## LINQ & Data Queries
- Build a dashboard: top 5 customers by spending, most sold products, customers with no orders (→ LINQ GroupBy, Join, aggregates)  
- Implement a “Customers who ordered Item X but not Item Y” report (→ LINQ set operations: Except/Any)  
- Add a feature: Orders placed in last 7 days grouped by hour (→ LINQ grouping, DateTime manipulation)  
- Build an “Order Search” page where filters (date range, min total, customer substring, item ID) can all be optional (→ Dynamic predicate building)  

## Architecture & Patterns
- Split reads vs writes with CQRS (→ Mediator pattern, separate models)  
- Introduce Unit of Work so creating an order + payment is atomic (→ Transactions)  
- When an order is placed, automatically deduct stock and send confirmation (→ Domain events)  
- Add dependency injection: swap between InMemory vs SQL repository (→ DI container, interfaces)  
- Use DTOs/ViewModels instead of exposing EF entities directly in views (→ Encapsulation, security)  

## ASP.NET MVC Internals
- Create a custom model binder for complex nested forms (→ Model binding pipeline)  
- Add a custom validation attribute `[FutureDate]` that ensures `DeliveryDate > Today` (→ Attributes, IValidatableObject)  
- Write an Action Filter `[LogExecutionTime]` that times each controller method (→ Filters, middleware)  
- Add TempData + RedirectToAction pattern for “success messages” after Create/Edit (→ MVC state handling)  
- Secure your forms against overposting (→ Bind attribute, input DTOs)  

## Performance & Scaling
- Add paging + sorting to order list (→ IQueryable, deferred execution)  
- Use AsNoTracking for read-only queries (→ EF performance tuning)  
- Implement a cache for the product catalog that expires every 5 minutes (→ MemoryCache)  
- Run a stress test: simulate 100k rows and optimize slow queries (→ Benchmarking, profiling)  
- Stream a CSV export of orders with 100k+ rows (→ Reflection + streaming response)  

## Async & Parallelism
- Convert all DB queries to async/await (→ EF async methods)  
- Add support for cancellation tokens on long queries (→ Graceful cancellation)  
- Create a background task that runs nightly to archive old orders (→ Async tasks, hosted services)  
- Run multiple API calls in parallel (→ Task.WhenAll)  
- Demonstrate thread-safety issues by caching mutable objects and fix with immutability (→ Locks, immutable collections)  

## Security
- Implement claims-based authorization: only managers can edit orders (→ Identity, claims)  
- Prevent mass assignment by restricting bound properties (→ Bind attribute, AutoMapper)  
- Encrypt sensitive fields like credit card numbers at rest (→ Data protection APIs)  
- Add anti-forgery tokens to forms (→ CSRF prevention)  
- Log failed login attempts and throttle after 5 (→ Security + middleware)  

## Testing
- Unit test a service using a mocked repository (→ Moq, dependency injection)  
- Integration test with EF InMemory provider (→ Simulate DB)  
- Re-run tests using SQLite InMemory to catch relational quirks (→ Realistic DB)  
- Write a test for concurrency conflicts (→ Simulate two saves on same row)  
- Write a test that validates transactions roll back on failure (→ EF transaction test)  

## Reflection & Advanced Language Features
- Write a CSV exporter that uses reflection + attributes `[CsvExport]` (→ Reflection)  
- Add a generic comparer that sorts by any property using reflection (→ IComparer<T>, reflection)  
- Build a plugin system: load custom tax calculators from DLLs at runtime (→ Reflection, Assembly.Load)  
- Implement a custom attribute `[Audit]` that logs property changes when saving (→ Attributes + reflection)  
- Build a dynamic LINQ query builder (→ Expression trees)  

 
---
# Markdown Synthax
Heading	
# H1
## H2
### H3

Bold	**bold text**

Italic	*italicized text*

Blockquote > blockquote

Ordered List	
1. First item
2. Second item
3. Third item

Unordered List	
- First item
- Second item
- Third item

Code
`code`

Horizontal Rule
---

Link
[title](https://www.example.com)

Image
![alt text](image.jpg)
