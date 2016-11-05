using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace NWHarvest.Web.Models
{
    public class ListingsRepository
    {
        private ApplicationDbContext db;

        public ListingsRepository()
        {
            this.db = new ApplicationDbContext();
        }

        public ListingsRepository(ApplicationDbContext db)
        {
            this.db = db;
        }

        public IEnumerable<Listing> GetAllAvailable()
        {
            var currentDate = DateTime.Now;
            return (from b in db.Listings
                    where b.available == true
                        & b.expire_date >= currentDate
                        & b.Grower.IsActive == true
                    orderby b.id descending 
                    select b).ToList();
        }
        public IEnumerable<Listing> GetAvailableByGrower(int growerId)
        {
            var currentDate = DateTime.Now;
            return (from b in db.Listings
                    where b.available == true 
                        & b.Grower.Id == growerId
                        & b.expire_date >= currentDate
                    orderby b.id descending
                    select b).ToList();
        }

        public IEnumerable<Listing> GetAllUnavailableExpired(int daysSinceCreation)
        {
            var oldestAcceptableDate = DateTime.Now.AddDays(-daysSinceCreation);
            var currentDate = DateTime.Now;
            return (from b in db.Listings
                    where((b.available == false 
                        || b.expire_date < currentDate)
                        & b.expire_date > oldestAcceptableDate)
                    orderby b.id descending
                    select b).ToList();
        }
    
        public IEnumerable<Listing> GetUnavailableExpired(int growerId, int daysSinceCreation)
        {
            var oldestAcceptableDate = DateTime.Now.AddDays(-daysSinceCreation);
            var currentDate = DateTime.Now;
            return (from b in db.Listings
                    where ((b.available == false
                    || b.expire_date < currentDate)
                    & b.expire_date > oldestAcceptableDate
                    & b.Grower.Id == growerId)
                    orderby b.id descending
                    select b).ToList();
        }

        public IEnumerable<Listing> GetClaimedByFoodBank(int foodBankId, int daysSinceCreation)
        {
            var oldestAcceptableDate = DateTime.Now.AddDays(-daysSinceCreation);    
            return (from b in db.Listings
                    where (b.available == false
                    & b.expire_date > oldestAcceptableDate
                    & b.FoodBank.Id == foodBankId)
                    orderby b.id descending
                    select b).ToList();
        }

        public IEnumerable<PickupLocation> GetAllPickupLocations(int growerId)
        {
            var query = (from pl in db.PickupLocations
                         where (pl.Grower.Id == growerId)
                         select pl);
            var queryList = query.ToList();
            return queryList;
        }
    }
}