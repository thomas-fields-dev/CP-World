namespace CpWorld.Services
{
    using CpWorld.Enums;
    using CpWorld.Infrastructure;
    using CpWorld.Models;
    using CpWorld.ViewModel;
    using Microsoft.EntityFrameworkCore;

    public static class OrderService
    {
        public static DetailsViewModel ModalPopup(int orderId, CPWorldDbContent context)
        {
            DetailsViewModel currentView = new DetailsViewModel();
            var currentOrder = context.Orders.Where(o => o.OrderId == orderId).First();
            currentView.Order = currentOrder;
            currentView.Message = $"Are you sure you would like to delete order {currentOrder.OrderId}?";
            currentView.Disabled = ButtonNames.Delete.ToString();
            return currentView;
        }

        public static ReportsViewModel GenerateReport(int selectedReport, CPWorldDbContent context)
        {
            object generatedReport = new object();
            switch (selectedReport)
            {
                case 1:
                    Dictionary<string, decimal> top5Customers = new Dictionary<string, decimal>();
                    var topSpenderQuery = (from o in context.Orders
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
                    var mostSoldQuery = (from oi in context.OrderItems
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
                    var reportOrdersQuery = from o in context.Orders.Include(o => o.OrderItems)
                                            where o.OrderItems.Count == 0
                                            select o;
                    List<Order> reportOrders = reportOrdersQuery.ToList();
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

        public static CreateViewModel CreateOrder(CreateViewModel newOrder, CPWorldDbContent context, ILogger logger)
        {
            newOrder.OrderDate = DateTime.Now;

            // what is a more elegant way to do this? xD
            Order order = new Order();
            List<OrderItem> orderItems = new List<OrderItem>();
            order.OrderDate = newOrder.OrderDate;
            order.CustomerName = newOrder.CustomerName;

            List<Item> allItems = context.Item.ToList();

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
                            var taggedItem = allItems.First(i => i.ItemId == newOrderedItem.ItemId);
                            itemsNotAvailable.Add(taggedItem.ProductName, taggedItem.QuantityAvailable);
                        }
                        else
                        {
                            OrderItem oi = new OrderItem();
                            oi.Quantity = (int)newOrderedItem.Quantity;
                            oi.Item = allItems.First(i => i.ItemId == newOrderedItem.ItemId);
                            orderItems.Add(oi);
                        }
                    }
                }

                order.OrderItems = orderItems;
            }

            if (itemsNotAvailable.Count == 0)
            {
                context.Orders.Add(order);
                try
                {
                    int rowsAdded = context.SaveChanges();
                    if (rowsAdded != 0)
                    {
                        newOrder.OrderItems = order.OrderItems;
                        newOrder.OrderId = context.OrderItems.Max(oi => oi.OrderItemId);
                        newOrder.Response = $"Order {newOrder.OrderId} Created on {newOrder.OrderDate}";
                    }
                    else
                    {
                        newOrder.Response = $"Order Failed to Save :(";
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex.Message);
                    if (ex.InnerException != null)
                    {
                        newOrder.Response = ex.InnerException.Message;
                        logger.LogError(ex.InnerException.Message);
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
                newOrder.Response += string.Join("\n", itemsNotAvailable.Select(i => i.Key + " - Only " + i.Value + " in stock"));
            }

            return newOrder;
        }

        public static EditViewModel EditOrder(EditViewModel model, CPWorldDbContent context)
        {
            List<Item> items = context.Item.ToList();
            List<OrderItem> orderedItems = new List<OrderItem>();
            if (model.OrderedItems != null)
            {
                foreach (var item in model.OrderedItems)
                {
                    if (item.Quantity != null && item.Quantity != 0)
                    {
                        OrderItem orderedItem = new OrderItem();
                        orderedItem.Quantity = (int)item.Quantity;
                        orderedItem.Item = items.First(i => i.ItemId == item.ItemId);
                        orderedItems.Add(orderedItem);
                    }
                }
            }

            Order originalOrder = context.Orders.First(o => o.OrderId == model.OrderId);
            originalOrder.OrderItems = orderedItems;
            originalOrder.CustomerName = model.CustomerName;
            context.Update(originalOrder);
            int rows = context.SaveChanges();

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

        public static EditViewModel FetchEditableOrder(int? orderId, CPWorldDbContent context)
        {
            var existingOrder = context.Orders.Where(o => o.OrderId == orderId).First();
            EditViewModel view = new EditViewModel();

            view.OrderItems = existingOrder.OrderItems;
            view.OrderId = existingOrder.OrderId;
            view.OrderDate = existingOrder.OrderDate;
            view.OrderStatus = existingOrder.OrderStatus;
            view.CustomerName = existingOrder.CustomerName;

            return view;
        }

        public static DetailsViewModel DeleteOrder(int orderId, CPWorldDbContent context)
        {
            DetailsViewModel detailsView = new DetailsViewModel();
            var orderToDelete = context.Orders.Where(o => o.OrderId == orderId).First();
            context.Remove(orderToDelete);
            int rows = context.SaveChanges();
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

        public static OrderViewModel GetAllOrders(string? searchTerm, OrderStatus? orderStatus, int? currentPage, CPWorldDbContent context)
        {
            var orders = context.Orders.Include(o => o.OrderItems).ThenInclude(oi => oi.Item).ToList();

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
            int pages = (int)decimal.Round((decimal)orders.Count / resultsPerPage, MidpointRounding.AwayFromZero);
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
