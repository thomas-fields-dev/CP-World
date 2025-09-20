namespace CpWorld.ViewModel
{
    using CpWorld.Models;

    public class DetailsViewModel
    {
        public DetailsViewModel()
        {
            this.Order = new Order();
        }

        public Order Order { get; set; }

        public string Message { get; set; } = string.Empty;

        public string Disabled { get; set; } = string.Empty;
    }
}