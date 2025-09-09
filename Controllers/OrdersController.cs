using CpWorld.Infrastructure;
using CpWorld.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using CpWorld.ViewModel;
using CpWorld.Emums;

namespace CpWorld.Controllers;

public class OrdersController : Controller
{
    private readonly ILogger<OrdersController> _logger;
    private readonly CPWorldDbContent _context;

    //3. Controller Enhancements
    //Index:
    //Add filtering(search orders by CustomerName or filter by OrderStatus).
    //Add pagination(skip/take).
    //Details:
    //Show computed TotalAmount.
    //Create:
    //Populate a dropdown with Item list(from DB) to add to an order.
    //Ensure stock check: don’t allow ordering more than QuantityAvailable.
    //Edit:
    //Allow modifying Order (add/remove OrderItems).
    //Delete:
    //Confirm cascade delete works.
    public OrdersController(ILogger<OrdersController> logger, CPWorldDbContent context)
    {
        _logger = logger;
        _context = new CPWorldDbContent();
    }

    //Controller — OrdersController
    //Index() → list all orders with customer + order date (include items count).
    //##################
    //Add filtering(search orders by CustomerName or filter by OrderStatus).
    //Add pagination(skip/take).
    public IActionResult Index(string? searchTerm, OrderStatus? orderStatus, int? currentPage)
    {
        _logger.Log(LogLevel.Information, "Connection to Database started");
        var orders = _context.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.Item).ToList();

        if (searchTerm != null)
        {
            orders = orders.Where(o => o.CustomerName.Contains(searchTerm)).ToList();
        }
        if (orderStatus != OrderStatus.All)
        {
            switch (orderStatus)
            {
                case OrderStatus.Cancelled:
                    orders = orders.Where(o => o.OrderStatus == OrderStatus.Cancelled).ToList();
                    break;
                case OrderStatus.Shipped:
                    orders = orders.Where(o => o.OrderStatus == OrderStatus.Shipped).ToList();
                    break;
                case OrderStatus.Pending:
                    orders = orders.Where(o => o.OrderStatus == OrderStatus.Pending).ToList();
                    break;
                default:
                    break;
            }
        }

        int resultsPerPage = 2;
        // Decimal.Round always rounds down xD and if there is no floating point interger for 0.5 in Math.Round it will evaluate to 0 unless you AwayFromZero
        int pages = (int)Math.Round((decimal)orders.Count / resultsPerPage, MidpointRounding.AwayFromZero);
        int[] pageNumbers = new int[pages];
        for (int i = 0; i < pages; i++)
        {
            pageNumbers[i] = i + 1;
        }

        if (currentPage == null)
        {
            currentPage = 1;
        }

        int resultsToSkip = resultsPerPage * ((int)currentPage - 1);
        orders = orders.Skip(resultsToSkip).Take(resultsPerPage).ToList();

        OrderViewModel homeViewModel = new OrderViewModel();
        homeViewModel.Pages = pageNumbers.ToList();
        homeViewModel.Response = orders;
        homeViewModel.CurrentPage = (int)currentPage;
        if (orders.Count == 0)
        {
            homeViewModel.Message = "No Orders to Display";
        }
        return View(homeViewModel);
    }

    //Details(int id) → show one order with all its items.
    public IActionResult Details(int? id)
    {
        DetailsViewModel order = new DetailsViewModel();
        _logger.Log(LogLevel.Information, "Connection to Database started");
        if (id != null)
        {
            order.Order = _context.Orders.Where(o => o.OrderId == id).Include(o => o.OrderItems).ThenInclude(oi => oi.Item).First();
        }
        return View(order);
    }

    public IActionResult Item(int? id)
    {
        Item item = _context.Item.Where(i => i.ItemId == id && i.isActive).First();
        if (item != null) return View(item);
        else return View();
    }

    //Create(Order order) (POST) → save new order with multiple items into EF.
    [HttpPost]
    public IActionResult Create(CreateViewModel newOrder)
    {
        // ModelState refers to the bound Model being passed in, in this case CreateViewModel
        if (ModelState.IsValid)
        {
            newOrder.OrderDate = DateTime.Now;

            // what is a more elegant way to do this? xD
            Order order = new Order();
            List<OrderItem> orderItems = new List<OrderItem>();
            order.OrderDate = newOrder.OrderDate;
            order.CustomerName = newOrder.CustomerName;
            if (newOrder.OrderedItems != null)
            {
                foreach (OrderItemViewModel newOrderedItem in newOrder.OrderedItems)
                {
                    if (newOrderedItem.Quantity != null && newOrderedItem.Quantity != 0)
                    {
                        OrderItem oi = new OrderItem();
                        oi.Quantity = (int)newOrderedItem.Quantity;
                        oi.Item = _context.Item.Where(i => i.ItemId == newOrderedItem.ItemId).First();
                        orderItems.Add(oi);
                    }
                }
                order.OrderItems = orderItems;
            }
            _context.Orders.Add(order);
            try
            {
                int rowsAdded = _context.SaveChanges();
                if (rowsAdded != 0)
                {
                    newOrder.OrderItems = order.OrderItems;
                    newOrder.OrderId = _context.OrderItems.Max(oi => oi.OrderItemId);
                    newOrder.Response = $"Order {newOrder.OrderId} Created on {newOrder.OrderDate}";
                }
                else
                {
                    newOrder.Response = $"Order Failed to Save :(";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                if (ex.InnerException != null)
                {
                    newOrder.Response = ex.InnerException.Message;
                    _logger.LogError(ex.InnerException.Message);
                }
                else
                {
                    newOrder.Response = ex.Message;
                }
                newOrder.OrderDate = DateTime.MinValue;
                newOrder.OrderId = 0;
            }
        }
        List<Item> items = new List<Item>();
        foreach (Item item in _context.Item.Where(i => i.isActive))
        {
            items.Add(item);
        }
        ViewBag.Items = items;
        return View(newOrder);
    }

    //Create() (GET) → return form.
    public IActionResult Create()
    {
        CreateViewModel createViewModel = new CreateViewModel();
        List<Item> items = new List<Item>();
        foreach (Item item in _context.Item.Where(i => i.isActive))
        {
            items.Add(item);
        }
        ViewBag.Items = items;

        return View(createViewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}


//Requirements
//Models(Code First)
//Order
//OrderId(PK, int)
//CustomerName(string, required, max length 100)
//OrderDate(DateTime)
//Items(ICollection<OrderItem>)

//OrderItem
//OrderItemId(PK, int)
//ProductName(string, required, max length 100)
//Quantity(int)
//Price(decimal)
//Navigation → OrderId(FK), Order

//DbContext
//Create AppDbContext with DbSet<Order> and DbSet<OrderItem>.
//Configure relationships so deleting an Order also deletes its Items (cascade).

//Controller — OrdersController
//Index() → list all orders with customer + order date (include items count).
//Details(int id) → show one order with all its items.
//Create() (GET) → return form.
//Create(Order order) (POST) → save new order with multiple items into EF.

//Views
//Index.cshtml → Table of Orders (Id, Customer, Date, ItemCount).
//Details.cshtml → Show Order + loop through Items.
//Create.cshtml → Form to add order + dynamically add at least 2 items.

//Extras
//Use EF migrations (Add-Migration, Update-Database).
//Use Include() when loading related data.
//Bonus: Add basic server-side validation with[Required], [StringLength], [Range].

//👉 Deliverable: Write
//Models
//DbContext
//Controller (with EF)
//Views (Index, Details, Create)

//##################################################################################
//🔥 Expanded Requirements for MVC + EF Interview Prep
//1. Models(Domain + Validation + Relationships)
//Order
//Add[Required], [StringLength] validation attributes.
//Add computed property → TotalAmount (sum of item prices).
//Add status field: OrderStatus(enum: Pending, Shipped, Cancelled).
//OrderItem
//Validation: [Range(1, 100)] for Quantity.
//Remove raw ProductName + Price duplication, instead add:
//ItemId(FK to Item table).
//Navigation: Item(so OrderItem links to catalog items).
//Item
//Already exists in your schema(ItemId, ProductName, QuantityAvailable, Price).
//Bonus: Track IsActive so discontinued items don’t appear in dropdowns.
//👉 Interview win: Show you understand normalization(don’t store product/price repeatedly in OrderItem → reference Item).

//2. DbContext
//Add DbSet<Item>.
//Configure:
//Order → OrderItems relationship with cascade delete.
//OrderItem → Item(FK).
//Use Fluent API in OnModelCreating to configure relationships + constraints.
//(Shows you can go beyond data annotations.)

//3. Controller Enhancements
//Index:
//Add filtering(search orders by CustomerName or filter by OrderStatus).
//Add pagination(skip/take).
//Details:
//Show computed TotalAmount.
//Create:
//Populate a dropdown with Item list(from DB) to add to an order.
//Ensure stock check: don’t allow ordering more than QuantityAvailable.
//Edit:
//Allow modifying Order (add/remove OrderItems).
//Delete:
//Confirm cascade delete works.
//👉 Interview win: If you mention async actions (await _context.Orders.Include(...).ToListAsync()) you’ll look extra sharp.

//4. Views
//Index.cshtml
//Add search box + filter dropdown for status.
//Show pagination links.
//Details.cshtml
//Show order details, items, total, and current stock left (join with Item table).
//Create.cshtml
//Dropdowns for products.
//Dynamic JavaScript to add/remove rows for items.
//Validation messages(asp-validation-for).
//Edit.cshtml
//Similar to create but pre-populated.

//5. Extras(Interview Show-Offs)
//Validation:
//Client + server side validation with ModelState.IsValid.
//Error handling:
//Graceful handling when order not found(return NotFound()).
//Dependency Injection:
//Abstract EF calls behind IOrderRepository.
//Services:
//Business logic(like checking stock availability) in a service layer.
//AutoMapper:
//Use DTOs/ViewModels to avoid exposing EF entities directly.
//Unit Tests:
//Mock DbContext(with InMemory provider).
//Test controller actions.

//6. Advanced Interview Topics
//Lazy vs Eager vs Explicit loading
//.Include() → eager loading.
//Lazy loading pitfalls (N+1).
//When to use.Load().
//Performance optimizations
//.AsNoTracking() for read-only queries.
//Pagination(skip/take).
//Security
//Prevent overposting(use[Bind] or ViewModels).
//Validate input before saving to DB.
//Architecture
//Move logic to Services/Repositories.
//Discuss Repository vs Unit of Work patterns.
