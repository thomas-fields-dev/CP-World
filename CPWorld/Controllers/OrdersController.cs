using CpWorld.Enums;
using CpWorld.Infrastructure;
using CpWorld.Models;
using CpWorld.Services;
using CpWorld.ViewModel;
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
        _context = context;
    }

    public IActionResult Index(string? searchTerm, OrderStatus? orderStatus, int? currentPage)
    {
        _logger.Log(LogLevel.Information, "Connection to Database started");
        OrderService service = new OrderService();
        OrderViewModel homeViewModel = service.GetAllOrders(searchTerm, orderStatus, currentPage, _context);
        return View(homeViewModel);
    }

    public IActionResult Details(int? orderId)
    {
        DetailsViewModel order = new DetailsViewModel();
        _logger.Log(LogLevel.Information, "Connection to Database started");
        if (orderId != null)
        {
            order.Order = _context.Orders.Where(o => o.OrderId == orderId).Include(o => o.OrderItems).ThenInclude(oi => oi.Item).First();
        }
        return View(order);
    }

    public IActionResult DisplayModal(int orderId)
    {
        OrderService service = new OrderService();
        var currentView = service.ModalPopup(orderId, _context);
        return View("Details", currentView);
    }

    public IActionResult Delete(int orderId)
    {
        OrderService service = new OrderService();
        var detailsView = service.DeleteOrder(orderId, _context);
        return View("Details", detailsView);
    }

    public IActionResult Item(int? id)
    {
        Item item = _context.Item.Where(i => i.ItemId == id && i.isActive).First();
        if (item != null) return View(item);
        else return View();
    }

    public IActionResult Edit(int? orderId)
    {
        OrderService orderService = new OrderService();
        var view = orderService.FetchEditableOrder(orderId, _context);

        var items = _context.Item.Where(i => i.isActive).ToList();
        ViewBag.Items = items;

        return View("Edit", view);
    }

    [HttpPost]
    public IActionResult Edit(EditViewModel model)
    {
        OrderService service = new OrderService();
        var viewModel = service.EditOrder(model, _context);

        ViewBag.Items = _context.Item.Where(i => i.isActive).ToList();
        return View("Edit", viewModel);

        //additional checks
        //view.Response = $"Order {existingOrder.OrderId} has been editied.";
        //view.Response = $"New items added to order {existingOrder.OrderId} edited.";
        //view.Response = $"Items removed from order {existingOrder.OrderId}.";
    }

    [HttpPost]
    public IActionResult Create(CreateViewModel newOrder)
    {
        if (ModelState.IsValid)
        {
            OrderService service = new OrderService();
            newOrder = service.CreateOrder(newOrder, _context, _logger);
        }

        List<Item> items = _context.Item.Where(i => i.isActive).ToList();
        ViewBag.Items = items;

        return View(newOrder);
    }

    public IActionResult Create()
    {
        CreateViewModel createViewModel = new CreateViewModel();
        List<Item> items = _context.Item.Where(i => i.isActive).ToList();
        ViewBag.Items = items;

        return View(createViewModel);
    }

    public IActionResult Reports()
    {
        var reportsViewModel = new ReportsViewModel();
        return View(reportsViewModel);
    }

    [HttpPost]
    public IActionResult Reports(int selectedReport)
    {
        OrderService service = new OrderService();
        var reportsViewModel = service.GenerateReport(selectedReport, _context);
        return View(reportsViewModel);
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
