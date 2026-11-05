using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SagarCourierApp.Models;


namespace SagarCourierApp.ApiController
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShipmentApiController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public ShipmentApiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private bool TryGetCurrentUser(out int userId,out int roleId)
        {
            userId = 0;
            roleId = 0;

            string? userIdValue = HttpContext.Session.GetString("UserId");
            string? roleIdValue = HttpContext.Session.GetString("RoleId");

            return int.TryParse(userIdValue, out userId)
                 && int.TryParse(roleIdValue, out roleId)
                 && (roleId == 1 || roleId == 2);


        }

        [HttpPost]
        public IActionResult CreateShipment([FromBody] Shipment shipment)
        {
            if(!TryGetCurrentUser(out int userId,out int roleId))
            {
                return Unauthorized(new { message = "Please login first." });
            }

            if(string.IsNullOrWhiteSpace(shipment.SenderName) ||
               string.IsNullOrWhiteSpace(shipment.ReceiverName) ||
               string.IsNullOrWhiteSpace(shipment.ReceiverPhone) ||
               string.IsNullOrWhiteSpace(shipment.PickupAddress) ||
               string.IsNullOrWhiteSpace(shipment.DeliveryAddress))
            {
                return BadRequest(new
                {
                    message = "All shipment fields are required."
                });
            }

            if(shipment.Weight <= 0)
            {
                return BadRequest(new
                {
                    message = "Weight must be greater than zero."
                });
            }
            string connectionString =
                   _configuration.GetConnectionString("DefaultConnection");

            string query = @"INSERT INTO Shipments(TrackingNumber,SenderName,ReceiverName,
                            ReceiverPhone,PickupAddress,DeliveryAddress,
                            Weight,UserId) VALUES(@TrackingNumber,@SenderName,@ReceiverName,
                            @ReceiverPhone,@PickupAddress,@DeliveryAddress,
                            @Weight,@UserId)";
           
            return Ok();
        }
    }

}

