namespace NWHarvest.Web.Models
{
    public static class UserRoles
    {
        public static string AdministratorRole
        {
            get
            {
                return "Admin";
            }
        }
        public static string GrowerRole
        {
            get
            {
                return "Grower";
            }
        }
        public static string FoodBankRole
        {
            get
            {
                return "Food Program";
            }
        }

        public static string Administrator
        {
            get
            {
                return "admin@northwestharvest.com";
            }
        }
    }
}