using CpWorld.Enums;
using CpWorld.Infrastructure;
using CpWorld.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace CpWorld.Services
{
    public class OrderService
    {
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
