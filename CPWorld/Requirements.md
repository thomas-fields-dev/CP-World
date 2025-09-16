## Requirements
**Models(Code First)**
- Order
- OrderId(PK, int)
- CustomerName(string, required, max length 100)
- OrderDate(DateTime)
- Items(ICollection<OrderItem>)
___
**OrderItem**
- OrderItemId(PK, int)
- ProductName(string, required, max length 100)
- Quantity(int)
- Price(decimal)
- Navigation → OrderId(FK), Order
___
**DbContext**
- Create AppDbContext with DbSet<Order> and DbSet<OrderItem>.
- Configure relationships so deleting an Order also deletes its Items (cascade).
___
**Controller — OrdersController**
- Index() → list all orders with customer + order date (include items count).
- Details(int id) → show one order with all its items.- 
- Create() (GET) → return form.
- Create(Order order) (POST) → save new order with multiple items into EF.
___
**Views**
- Index.cshtml → Table of Orders (Id, Customer, Date, ItemCount).
- Details.cshtml → Show Order + loop through Items.
- Create.cshtml → Form to add order + dynamically add at least 2 items.
___
**Extras**
- Use EF migrations (Add-Migration, Update-Database).
- Use Include() when loading related data.
- Bonus: Add basic server-side validation with[Required], [StringLength], [Range].
___
**👉 Deliverable: Write**
- Models
- DbContext
- Controller (with EF)
- Views (Index, Details, Create)
___
## 🔥 Expanded Requirements for MVC + EF Interview Prep

### 1. Models(Domain + Validation + Relationships)
**Order**
- Add[Required], [StringLength] validation attributes.
- Add computed property → TotalAmount (sum of item prices).
- Add status field: OrderStatus(enum: Pending, Shipped, Cancelled).

**OrderItem**
- Validation: [Range(1, 100)] for Quantity.
- Remove raw ProductName + Price duplication, instead add:
- ItemId(FK to Item table).
- Navigation: Item(so OrderItem links to catalog items).

**Item**
- Already exists in your schema(ItemId, ProductName, QuantityAvailable, Price).
- Bonus: Track IsActive so discontinued items don’t appear in dropdowns.
- 👉 Interview win: Show you understand normalization(don’t store product/price repeatedly in OrderItem → reference Item).
___
### 2. DbContext
**Add `DbSet<Item>` Configure:**
- Order → OrderItems relationship with cascade delete.
- OrderItem → Item(FK).
- Use Fluent API in OnModelCreating to configure relationships + constraints.
- (Shows you can go beyond data annotations.)
___
### 3. Controller Enhancements
**Index:**
- Add filtering(search orders by CustomerName or filter by OrderStatus).
- Add pagination(skip/take).

**Details:**
- Show computed TotalAmount.

**Create:**
- Populate a dropdown with Item list(from DB) to add to an order.
- Ensure stock check: don’t allow ordering more than QuantityAvailable.

**Edit:**
- Allow modifying Order (add/remove OrderItems).

**Delete:**
- Confirm cascade delete works.

**👉 Interview win: If you mention async actions (await _context.Orders.Include(...).ToListAsync()) you’ll look extra sharp.**
___
### 4. Views
**Index.cshtml**
- Add search box + filter dropdown for status.
- Show pagination links.

**Details.cshtml**
- Show order details, items, total, and current stock left (join with Item table).

**Create.cshtml**
- Dropdowns for products.
- Dynamic JavaScript to add/remove rows for items.
- Validation messages(asp-validation-for).

**Edit.cshtml**
- Similar to create but pre-populated.
___
### 5. Extras(Interview Show-Offs)
**Validation:**
- Client + server side validation with ModelState.IsValid.

**Error handling:**
- Graceful handling when order not found(return NotFound()).

**Dependency Injection:**
- Abstract EF calls behind IOrderRepository.

**Services:**
- Business logic(like checking stock availability) in a service layer.

**AutoMapper:**
- Use DTOs/ViewModels to avoid exposing EF entities directly.

**Unit Tests:**
- Mock DbContext(with InMemory provider).
- Test controller actions.
___
### 6. Advanced Interview Topics
**Lazy vs Eager vs Explicit loading**
- .Include() → eager loading.
- Lazy loading pitfalls (N+1).
- When to use.Load().

**Performance optimizations**
- .AsNoTracking() for read-only queries.
- Pagination(skip/take)

**Security**
- Prevent overposting(use[Bind] or ViewModels).
- Validate input before saving to DB.

**Architecture**
- Move logic to Services/Repositories.
- Discuss Repository vs Unit of Work patterns.

---
### Markup Synthax
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
