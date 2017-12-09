namespace NWHarvest.Web.Models
{
    // deprecated use UserRole enum
    public static class UserRoles
    {
        public static string AdministratorRole
        {
            get
            {
                return "Administrator";
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
                return "growingconnections@northwestharvest.org";
            }
        }
    }
}