using CpWorld.Enums;
using CpWorld.Infrastructure;
using CpWorld.Models;
using CpWorld.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace CpWorld.Services
{
    public class OrderService
    {
        public DetailsViewModel ModalPopup(int orderId, CPWorldDbContent _context)
        {
            DetailsViewModel currentView = new DetailsViewModel();
            var currentOrder = _context.Orders.Where(o => o.OrderId == orderId).First();
            currentView.Order = currentOrder;
            currentView.Message = $"Are you sure you would like to delete order {currentOrder.OrderId}?";
            currentView.Disabled = ButtonNames.Delete.ToString();
            return currentView;
        }
        public ReportsViewModel GenerateReport(int selectedReport, CPWorldDbContent _context)
        {
            object generatedReport = new object();
            switch (selectedReport)
            {
                case 1:
                    Dictionary<string, decimal> top5Customers = new Dictionary<string, decimal>();
                    var topSpenderQuery = (from o in _context.Orders
                                      .Include(oi => oi.OrderItems)
                                      .ThenInclude(i => i.Item)
                                      .AsEnumerable()
                                           group o by o.CustomerName into grp
                                           select new { CustomerName = grp.Key, Total = grp.Sum(t => t.TotalAmount) }
                                           into n
                                           orderby n.Total descending
                                           select n).Take(5).ToList();
                    foreach (var report in topSpenderQuery)
                    {
                        top5Customers.Add(report.CustomerName, report.Total);
                    }
                    generatedReport = top5Customers;
                    break;
                case 2:
                    Dictionary<int, string> mostSoldProducts = new Dictionary<int, string>();
                    var mostSoldQuery = (from oi in _context.OrderItems
                                         where oi.Item != null
                                         group oi by oi.Item.ProductName into i
                                         select new { ItemName = i.Key, Sold = i.Sum(f => f.Quantity) }
                                         into r
                                         orderby r.Sold descending
                                         select r)
                                .Take(5).ToList();
                    var results = mostSoldQuery;
                    foreach (var item in results)
                    {
                        mostSoldProducts.Add(item.Sold, item.ItemName);
                    }
                    generatedReport = mostSoldProducts;
                    break;
                case 3:
                    List<Order> reportOrders = new List<Order>();
                    var reportOrdersQuery = from o in _context.Orders.Include(o => o.OrderItems)
                                            where o.OrderItems.Count == 0
                                            select o;
                    reportOrders = reportOrdersQuery.ToList();
                    generatedReport = reportOrders;
                    break;
                default:
                    break;
            }
            var reportsViewModel = new ReportsViewModel();
            reportsViewModel.SelectedReport = selectedReport;
            reportsViewModel.GeneratedReport = generatedReport;
            return reportsViewModel;
        }

        public CreateViewModel CreateOrder(CreateViewModel newOrder, CPWorldDbContent _context, ILogger _logger)
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
            return newOrder;
        }

        public EditViewModel EditOrder(EditViewModel model, CPWorldDbContent _context)
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
            return viewModel;
        }

        public EditViewModel FetchEditableOrder(int? orderId, CPWorldDbContent _context)
        {
            var existingOrder = _context.Orders.Where(o => o.OrderId == orderId).First();
            EditViewModel view = new EditViewModel();

            view.OrderItems = existingOrder.OrderItems;
            view.OrderId = existingOrder.OrderId;
            view.OrderDate = existingOrder.OrderDate;
            view.OrderStatus = existingOrder.OrderStatus;
            view.CustomerName = existingOrder.CustomerName;

            return view;
        }

        public DetailsViewModel DeleteOrder(int orderId, CPWorldDbContent _context)
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
            return detailsView;
        }

        public OrderViewModel GetAllOrders(string? searchTerm, OrderStatus? orderStatus, int? currentPage, CPWorldDbContent _context)
        {
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
            // if there is no floating point interger for 0.5 in Decimal.Round it will evaluate to 0 unless you AwayFromZero
            int pages = (int)Decimal.Round((decimal)orders.Count / resultsPerPage, MidpointRounding.AwayFromZero);
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

            return homeViewModel;
        }
    }
}
