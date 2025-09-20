namespace CpWorld.Controllers
{
    using System.Diagnostics;
    using CpWorld.Enums;
    using CpWorld.Infrastructure;
    using CpWorld.Models;
    using CpWorld.Services;
    using CpWorld.ViewModel;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;

    public class OrdersController : Controller
    {
        private readonly ILogger<OrdersController> logger;
        private readonly CPWorldDbContent context;

        public OrdersController(ILogger<OrdersController> logger, CPWorldDbContent context)
        {
            this.logger = logger;
            this.context = context;
        }

        public IActionResult Index(string? searchTerm, OrderStatus? orderStatus, int? currentPage)
        {
            if (this.ModelState.IsValid)
            {
                this.logger.Log(LogLevel.Information, "Connection to Database started");
                OrderViewModel homeViewModel = OrderService.GetAllOrders(searchTerm, orderStatus, currentPage, this.context);
                return this.View(homeViewModel);
            }
            else
            {
                return this.View();
            }
        }

        public IActionResult Details(int? orderId)
        {
            if (this.ModelState.IsValid)
            {
                DetailsViewModel order = new DetailsViewModel();
                this.logger.Log(LogLevel.Information, "Connection to Database started");
                if (orderId != null)
                {
                    order.Order = this.context.Orders.Where(o => o.OrderId == orderId).Include(o => o.OrderItems).ThenInclude(oi => oi.Item).First();
                }

                return this.View(order);
            }
            else
            {
                return this.View();
            }
        }

        public IActionResult DisplayModal(int orderId)
        {
            if (this.ModelState.IsValid)
            {
                var currentView = OrderService.ModalPopup(orderId, this.context);
                return this.View("Details", currentView);
            }
            else
            {
                return this.View();
            }
        }

        public IActionResult Delete(int orderId)
        {
            if (this.ModelState.IsValid)
            {
                var detailsView = OrderService.DeleteOrder(orderId, this.context);
                return this.View("Details", detailsView);
            }
            else
            {
                return this.View();
            }
        }

        public IActionResult Item(int? id)
        {
            if (this.ModelState.IsValid)
            {
                Item item = this.context.Item.Where(i => i.ItemId == id && i.IsActive).First();
                if (item != null)
                {
                    return this.View(item);
                }
                else
                {
                    return this.View();
                }
            }
            else
            {
                return this.View();
            }
        }

        public IActionResult Edit(int? orderId)
        {
            if (this.ModelState.IsValid)
            {
                var view = OrderService.FetchEditableOrder(orderId, this.context);

                var items = this.context.Item.Where(i => i.IsActive).ToList();
                this.ViewBag.Items = items;

                return this.View("Edit", view);
            }
            else
            {
                return this.View();
            }
        }

        [HttpPost]
        public IActionResult Edit(EditViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var viewModel = OrderService.EditOrder(model, this.context);

                this.ViewBag.Items = this.context.Item.Where(i => i.IsActive).ToList();
                return this.View("Edit", viewModel);
            }
            else
            {
                return this.View();
            }
        }

        [HttpPost]
        public IActionResult Create(CreateViewModel newOrder)
        {
            if (this.ModelState.IsValid)
            {
                newOrder = OrderService.CreateOrder(newOrder, this.context, this.logger);
            }

            List<Item> items = this.context.Item.Where(i => i.IsActive).ToList();
            this.ViewBag.Items = items;

            return this.View(newOrder);
        }

        public IActionResult Create()
        {
            CreateViewModel createViewModel = new CreateViewModel();
            List<Item> items = this.context.Item.Where(i => i.IsActive).ToList();
            this.ViewBag.Items = items;

            return this.View(createViewModel);
        }

        public IActionResult Reports()
        {
            var reportsViewModel = new ReportsViewModel();
            return this.View(reportsViewModel);
        }

        [HttpPost]
        public IActionResult Reports(int selectedReport)
        {
            if (this.ModelState.IsValid)
            {
                var reportsViewModel = OrderService.GenerateReport(selectedReport, this.context);
                return this.View(reportsViewModel);
            }
            else
            {
                return this.View();
            }
        }

        public IActionResult Privacy()
        {
            return this.View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return this.View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? this.HttpContext.TraceIdentifier });
        }
    }
}