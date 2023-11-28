using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CityInfo.API.Controllers
{
    [ApiController]
    //[Authorize]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    [Route("api/v{version:apiVersion}/cities")]
    public class CitiesController:ControllerBase
    {
        private readonly ICityInfoRepository _cityInfoRepository;
        private readonly IMapper _mapper;
        private readonly int maxCitiesPageSize = 20;

        public CitiesController(ICityInfoRepository cityInfoRepository
            ,IMapper mapper)
        {
            _cityInfoRepository = cityInfoRepository ??
                throw new ArgumentNullException(nameof(cityInfoRepository));
            _mapper = mapper ??
                throw new ArgumentNullException(nameof(mapper));
        }

        public CitiesDataStore CitiesDataStore { get; }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CityWithoutPointsOfInterestDto>>> GetCities(
            string? name,string? searchQuery, int pageNumber = 1, int pageSize = 10)
        {
            if(pageSize>maxCitiesPageSize)
            {
              pageSize = maxCitiesPageSize;
            }
            var (cityEntities,paginationMetaData) = await _cityInfoRepository.GetCitiesAsync(
                name, searchQuery,pageNumber,pageSize);
            Response.Headers.Add("X-Pagination",
                JsonSerializer.Serialize(paginationMetaData));
           return Ok(_mapper.Map<IEnumerable<CityWithoutPointsOfInterestDto>>(cityEntities));
        }

        /// <summary>
        /// Get a city by id 
        /// </summary>
        /// <param name="id">The id of the city go get </param>
        /// <param name="includePointsOfInterest">Wheter or not to include the points of interest</param>
        /// <returns></returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCity(int id,bool includePointsOfInterest=false) 
        {
            var city = await _cityInfoRepository.GetCityAsync(id,includePointsOfInterest);
            if (city == null)
            {
                return NotFound();
            }
            if(includePointsOfInterest)
            {
                return Ok(_mapper.Map<CityDto>(city));
            }
            return Ok(_mapper.Map<CityWithoutPointsOfInterestDto>(city));
        }
    }
}
