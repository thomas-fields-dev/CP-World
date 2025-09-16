using CpWorld.Infrastructure;
using CpWorld.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using CpWorld.ViewModel;
using CpWorld.Enums;
using CpWorld.Services;

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
        OrderService os = new OrderService();
        OrderViewModel homeViewModel = os.GetAllOrders(searchTerm, orderStatus, currentPage, _context);
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
        DetailsViewModel currentView = new DetailsViewModel();
        var currentOrder = _context.Orders.Where(o => o.OrderId == orderId).First();
        currentView.Order = currentOrder;
        currentView.Message = $"Are you sure you would like to delete order {currentOrder.OrderId}?";
        currentView.Disabled = ButtonNames.Delete.ToString();
        return View("Details", currentView);
    }

    public IActionResult Delete(int orderId)
    {
        DetailsViewModel detailsView = new DetailsViewModel();
        var orderToDelete = _context.Orders.Where(o => o.OrderId == orderId).First();
        _context.Remove(orderToDelete);
        int rows = _context.SaveChanges();
        if (rows != 0)
        {
            detailsView.Disabled = ButtonNames.Delete.ToString();
            detailsView.Message = $"Order {orderToDelete.OrderId} deleted!";
        }
        else
        {
            detailsView.Message = "There was an issue processing your request, please try again later.";
        }
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
        var existingOrder = _context.Orders.Where(o => o.OrderId == orderId).First();
        EditViewModel view = new EditViewModel();

        view.OrderItems = existingOrder.OrderItems;
        view.OrderId = existingOrder.OrderId;
        view.OrderDate = existingOrder.OrderDate;
        view.OrderStatus = existingOrder.OrderStatus;
        view.CustomerName = existingOrder.CustomerName;

        var items = _context.Item.Where(i => i.isActive).ToList();
        ViewBag.Items = items;

        return View("Edit", view);
    }

    [HttpPost]
    public IActionResult Edit(EditViewModel model)
    {
        List<Item> items = _context.Item.ToList();
        List<OrderItem> orderedItems = new List<OrderItem>();
        if (model.OrderedItems != null)
        {
            foreach (var item in model.OrderedItems)
            {
                if (item.Quantity != null && item.Quantity != 0)
                {
                    OrderItem orderedItem = new OrderItem();
                    orderedItem.Quantity = (int)item.Quantity;
                    orderedItem.Item = items.Where(i => i.ItemId == item.ItemId).First();
                    orderedItems.Add(orderedItem);
                }
            }
        }

        Order originalOrder = _context.Orders.Where(o => o.OrderId == model.OrderId).First();
        originalOrder.OrderItems = orderedItems;
        originalOrder.CustomerName = model.CustomerName;
        _context.Update(originalOrder);
        int rows = _context.SaveChanges();

        EditViewModel viewModel = new EditViewModel();
        viewModel.OrderItems = orderedItems;
        viewModel.OrderId = model.OrderId;
        viewModel.CustomerName = model.CustomerName;

        if (rows > 0)
        {
            viewModel.Response = $"Order {model.OrderId} updated.";
        }
        else
        {
            viewModel.Response = "Some error occured, please try again later";
        }

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
        // ModelState refers to the bound Model being passed in, in this case CreateViewModel
        if (ModelState.IsValid)
        {
            newOrder.OrderDate = DateTime.Now;

            // what is a more elegant way to do this? xD
            Order order = new Order();
            List<OrderItem> orderItems = new List<OrderItem>();
            order.OrderDate = newOrder.OrderDate;
            order.CustomerName = newOrder.CustomerName;

            List<Item> allItems = _context.Item.ToList();

            Dictionary<string, int> itemsNotAvailable = new Dictionary<string, int>();
            if (newOrder.OrderedItems != null)
            {
                foreach (OrderItemViewModel newOrderedItem in newOrder.OrderedItems)
                {
                    if (newOrderedItem.Quantity != null && newOrderedItem.Quantity != 0)
                    {
                        var isAvailable = allItems.Exists(ai => ai.QuantityAvailable >= newOrderedItem.Quantity && ai.ItemId == newOrderedItem.ItemId);
                        if (!isAvailable)
                        {
                            var taggedItem = allItems.Where(i => i.ItemId == newOrderedItem.ItemId).First();
                            itemsNotAvailable.Add(taggedItem.ProductName, taggedItem.QuantityAvailable);
                        }
                        else
                        {
                            OrderItem oi = new OrderItem();
                            oi.Quantity = (int)newOrderedItem.Quantity;
                            oi.Item = allItems.Where(i => i.ItemId == newOrderedItem.ItemId).First();
                            orderItems.Add(oi);
                        }
                    }
                }
                order.OrderItems = orderItems;
            }
            if (itemsNotAvailable.Count == 0)
            {
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
            else
            {
                newOrder.Response = "We can not fullfill this order:\n";
                newOrder.Response += String.Join("\n", itemsNotAvailable.Select(i => i.Key + " - Only " + i.Value + " in stock"));
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
