using System.ComponentModel.DataAnnotations.Schema;

namespace NWHarvest.Web.Models
{
    [Table("FoodBankClaim")]
    public class FoodBankClaim : BaseEntity
    {
        public FoodBankClaim()
        {
            this.Address = new Address();
        }

        public string Product { get; set; }
        public int Quantity { get; set; }
        public decimal CostPerUnit { get; set; }
        public Address Address { get; set; }
        public int GrowerId { get; set; }
        public int FoodBankId { get; set; }

        public Grower Grower { get; set; }
        public FoodBank FoodBank { get; set; }
    }
}