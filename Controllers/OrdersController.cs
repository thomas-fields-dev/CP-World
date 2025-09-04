using CpWorld.Infrastructure;
using CpWorld.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace CpWorld.Controllers;

public class OrdersController : Controller
{
    private readonly ILogger<OrdersController> _logger;
    private readonly CPWorldDbContent _context;

    public OrdersController(ILogger<OrdersController> logger, CPWorldDbContent context)
    {
        _logger = logger;
        _context = new CPWorldDbContent();
    }

    //Controller — OrdersController
    //Index() → list all orders with customer + order date (include items count).
    public IActionResult Index()
    {
        _logger.Log(LogLevel.Information, "Connection to Database started");
        var response = _context.Orders.Include(o => o.Items).ToList();

        // cant we just item count in the view?
        foreach (var item in response)
        {
            item.ItemCount = item.Items.Count();
        }
        OrderViewModel homeViewModel = new OrderViewModel();
        homeViewModel.Response = response;
        return View(homeViewModel);
    }

    //Details(int id) → show one order with all its items.
    public IActionResult Details(int? id)
    {
        DetailsViewModel order = new DetailsViewModel();
        _logger.Log(LogLevel.Information, "Connection to Database started");
        if (id != null)
        {
            order.Order = _context.Orders.Where(o => o.OrderId == id).Include(o => o.Items).First();
        }
        return View(order);
    }

    //Create(Order order) (POST) → save new order with multiple items into EF.
    [HttpPost]
    public IActionResult Create(CreateViewModel newOrder)
    {
        newOrder.OrderDate = DateTime.Now;
        
        // duh! this is Identity Column no need to set this manually but... isnt Linq great! xD
        //int largestOrderId = _context.Orders.Max(o => o.OrderId);
        //newOrder.OrderId = ++largestOrderId;

        // what is a more elegant way to do this? xD
        Order order = new Order();
        order.OrderDate = newOrder.OrderDate;
        order.CustomerName = newOrder.CustomerName;
        _context.Orders.Add(order);
        try
        {
            int rowsAdded = _context.SaveChanges();
            if (rowsAdded != 0)
            {
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
            newOrder.Response = ex.InnerException?.Message;
            newOrder.OrderDate = DateTime.MinValue;
            newOrder.OrderId = 0;
        }
        return View(newOrder);
    }

    //Create() (GET) → return form.
    public IActionResult Create()
    {
        CreateViewModel createViewModel = new CreateViewModel();
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
