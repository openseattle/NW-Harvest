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

        //Administrator Queries
        public IEnumerable<Listing> GetAllClaimedNotPickedUp(int daysSinceCreation)
        {
            var oldestAcceptableDate = DateTime.Now.AddDays(-daysSinceCreation);
            return (from b in db.Listings
                    where (b.available == false
                    & b.expire_date > oldestAcceptableDate
                    & b.IsPickedUp == false)
                    orderby b.id descending
                    select b).ToList();
        }

        public IEnumerable<Listing> GetAllUnavailableExpired(int daysSinceCreation)
        {
            var oldestAcceptableDate = DateTime.Now.AddDays(-daysSinceCreation);
            var currentDate = DateTime.Now;
            return (from b in db.Listings
                    where ((b.available == false
                        || b.expire_date < currentDate)
                        & b.expire_date > oldestAcceptableDate)
                    orderby b.id descending
                    select b).ToList();
        }

        //Grower Queries
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

        public IEnumerable<Listing> GetClaimedNotPickedNotExpiredUpByGrower(int growerId, int daysSinceCreation)
        {
            var currentDate = DateTime.Now;
            return (from b in db.Listings
                    where (b.available == false
                        & b.expire_date >= currentDate
                        & b.Grower.Id == growerId
                        & b.IsPickedUp == false)
                    orderby b.id descending
                    select b).ToList();
        }

        public IEnumerable<Listing> GetExpiredOrPickedUpByGrower(int growerId, int daysSinceCreation)
        {
            var oldestAcceptableDate = DateTime.Now.AddDays(-daysSinceCreation);
            var currentDate = DateTime.Now;
            return (from b in db.Listings
                    where (
                        (
                            (b.expire_date >= oldestAcceptableDate & b.expire_date < currentDate)
                            || b.IsPickedUp == true
                        )
                        & b.Grower.Id == growerId)
                    orderby b.id descending
                    select b).ToList();
        }

        //Food Bank Queries
        public IEnumerable<Listing> GetClaimedNotPickedUpNotExpiredByFoodBank(int foodBankId, int daysSinceCreation)
        {
            var currentDate = DateTime.Now;    
            return (from b in db.Listings
                    where (b.available == false
                        & b.expire_date >= currentDate 
                        & b.FoodBank.Id == foodBankId
                        & b.IsPickedUp == false)
                    orderby b.id descending
                    select b).ToList();
        }

        public IEnumerable<Listing> GetClaimedPickedUpByFoodBankNotPickedUp(int foodBankId, int daysSinceCreation)
        {
            var currentDate = DateTime.Now;
            var oldestAcceptableDate = DateTime.Now.AddDays(-daysSinceCreation);
            return (from b in db.Listings
                    where (
                        (
                            b.IsPickedUp == true
                            || (b.expire_date > oldestAcceptableDate & b.expire_date < currentDate)
                        )
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