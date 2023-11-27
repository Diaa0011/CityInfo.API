using CityInfo.API.Entities;

namespace CityInfo.API.Services
{
    public interface ICityInfoRepository
    {
        Task<bool> CityExistsAsync(int cityId);
        Task<IEnumerable<City>> GetCitiesAsync();
        Task<City> GetCityAsync(int cityId, bool includePointsOfInterest);
        Task<PointOfInterest> GetPointOfInterestForCityAsync(int cityId, int pointOfInterestId);
        Task<IEnumerable<PointOfInterest?>> GetPointOfInterestForCityAsync(int cityId);
        Task AddPointOfInterestForCityAsync(int cityId,PointOfInterest pointOfInterest);
        void DeletePointOfInterest(PointOfInterest pointOfInterest);
        Task<bool> SaveChangesAsync();
    }
}
