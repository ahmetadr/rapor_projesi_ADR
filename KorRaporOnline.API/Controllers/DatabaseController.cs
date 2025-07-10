using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KorRaporOnline.API.Models.DTOs;
using KorRaporOnline.API.Services.Interfaces;  // Doğru namespace'i ekledik

namespace KorRaporOnline.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DatabaseController : ControllerBase
    {
        private readonly IDatabaseService _databaseService;
        private readonly ILogger<DatabaseController> _logger;

        public DatabaseController(IDatabaseService databaseService, ILogger<DatabaseController> logger)
        {
            _databaseService = databaseService;
            _logger = logger;
        }

        // Diğer action metodları aynı kalacak
    }
}