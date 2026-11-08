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

        private bool TryGetCurrentUser(out int userId, out int roleId)
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
            // 1. Check logged-in user
            if (!TryGetCurrentUser(out int userId, out int roleId))
            {
                return Unauthorized(new
                {
                    message = "Please login first."
                });
            }

            // 2. Check null shipment object
            if (shipment == null)
            {
                return BadRequest(new
                {
                    message = "Invalid shipment data."
                });
            }

            // 3. Validate required fields
            if (string.IsNullOrWhiteSpace(shipment.SenderName) ||
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

            // 4. Validate weight
            if (shipment.Weight <= 0)
            {
                return BadRequest(new
                {
                    message = "Weight must be greater than zero."
                });
            }

            // 5. Get database connection string
            string connectionString =
                _configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return StatusCode(500, new
                {
                    message = "Database connection string is missing."
                });
            }

            // 6. Generate unique tracking number
            string trackingNumber = "SHP" +
                Guid.NewGuid().ToString("N").Substring(0, 13).ToUpper();

            // 7. Insert shipment query
            string query = @"INSERT INTO Shipments
                     (TrackingNumber, SenderName, ReceiverName,
                      ReceiverPhone, PickupAddress, DeliveryAddress,
                      Weight, UserId)
                     VALUES
                     (@TrackingNumber, @SenderName, @ReceiverName,
                      @ReceiverPhone, @PickupAddress, @DeliveryAddress,
                      @Weight, @UserId);

                     SELECT CAST(SCOPE_IDENTITY() AS INT);";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    // 8. Add SQL parameters
                    cmd.Parameters.AddWithValue("@TrackingNumber", trackingNumber);
                    cmd.Parameters.AddWithValue("@SenderName", shipment.SenderName);
                    cmd.Parameters.AddWithValue("@ReceiverName", shipment.ReceiverName);
                    cmd.Parameters.AddWithValue("@ReceiverPhone", shipment.ReceiverPhone);
                    cmd.Parameters.AddWithValue("@PickupAddress", shipment.PickupAddress);
                    cmd.Parameters.AddWithValue("@DeliveryAddress", shipment.DeliveryAddress);
                    cmd.Parameters.AddWithValue("@Weight", shipment.Weight);
                    cmd.Parameters.AddWithValue("@UserId", userId);

                    // 9. Execute query
                    con.Open();

                    int shipmentId = Convert.ToInt32(cmd.ExecuteScalar());

                    // 10. Return successful response
                    return Ok(new
                    {
                        success = true,
                        message = "Shipment created successfully!",
                        ShipmentId = shipmentId,
                        TrackingNumber = trackingNumber
                    });
                }
            }
        }


        [HttpGet]
        public IActionResult GetShipments()
        {
            // 1. Check logged-in user
            if (!TryGetCurrentUser(out int userId, out int roleId))
            {
                return Unauthorized(new
                {
                    message = "Please login first."
                });
            }

            // 2. Create shipment list
            List<Shipment> shipments = new List<Shipment>();

            // 3. Get connection string
            string connectionString =
                _configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return StatusCode(500, new
                {
                    message = "Database connection string is missing."
                });
            }

            // 4. SQL query
            string query = @"SELECT ShipmentId, TrackingNumber, SenderName, ReceiverName,
                     ReceiverPhone, PickupAddress, DeliveryAddress,
                     Weight, Status, CreatedAt, UserId
                     FROM Shipments";

            // Staff can view only their own shipments
            if (roleId == 2)
            {
                query += " WHERE UserId = @UserId";
            }

            // Show newest shipments first
            query += " ORDER BY ShipmentId DESC";

            // 5. Execute query
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    if (roleId == 2)
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                    }

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            shipments.Add(new Shipment
                            {
                                ShipmentId = Convert.ToInt32(reader["ShipmentId"]),
                                TrackingNumber = reader["TrackingNumber"].ToString()!,
                                SenderName = reader["SenderName"].ToString()!,
                                ReceiverName = reader["ReceiverName"].ToString()!,
                                ReceiverPhone = reader["ReceiverPhone"].ToString()!,
                                PickupAddress = reader["PickupAddress"].ToString()!,
                                DeliveryAddress = reader["DeliveryAddress"].ToString()!,
                                Weight = Convert.ToDecimal(reader["Weight"]),
                                Status = reader["Status"].ToString()!,
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                                UserId = Convert.ToInt32(reader["UserId"])
                            });
                        }
                    }
                }
            }

            // 6. Return shipment list
            return Ok(shipments);
        }



        [HttpGet("{id:int}")]
        public IActionResult GetShipmentById(int id)
        {
            if (!TryGetCurrentUser(out int userId, out int roleId))
            {
                return Unauthorized(new
                {
                    message = "Please login first."
                });
            }

            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Invalid shipment ID."
                });
            }

            Shipment shipment = null;

            string connectionString =
                _configuration.GetConnectionString("DefaultConnection");

            string query = @"SELECT ShipmentId, TrackingNumber, SenderName,
                     ReceiverName, ReceiverPhone, PickupAddress,
                     DeliveryAddress, Weight, Status, CreatedAt, UserId
                     FROM Shipments
                     WHERE ShipmentId = @ShipmentId";

            // Staff can access only their own shipments
            if (roleId == 2)
            {
                query += " AND UserId = @UserId";
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ShipmentId", id);

                    if (roleId == 2)
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                    }

                    con.Open();

                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            shipment = new Shipment
                            {
                                ShipmentId = Convert.ToInt32(reader["ShipmentId"]),
                                TrackingNumber = reader["TrackingNumber"].ToString()!,
                                SenderName = reader["SenderName"].ToString()!,
                                ReceiverName = reader["ReceiverName"].ToString()!,
                                ReceiverPhone = reader["ReceiverPhone"].ToString()!,
                                PickupAddress = reader["PickupAddress"].ToString()!,
                                DeliveryAddress = reader["DeliveryAddress"].ToString()!,
                                Weight = Convert.ToDecimal(reader["Weight"]),
                                Status = reader["Status"].ToString()!,
                                CreatedAt = Convert.ToDateTime(reader["CreatedAt"]),
                                UserId = Convert.ToInt32(reader["UserId"])
                            };
                        }
                    }
                }
            }

            if (shipment == null)
            {
                return NotFound(new
                {
                    message = "Shipment not found."
                });
            }

            return Ok(shipment);
        }


        [HttpPut("{id:int}")]

        public IActionResult UpdateShipment(int id, [FromBody] Shipment shipment)
        {
            if(!TryGetCurrentUser(out int userId,out int roleId))
            {
                return Unauthorized(new { message = "Please login first." });
            }

            if (shipment == null || 
                string.IsNullOrWhiteSpace(shipment.SenderName) ||
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
                    message = "Weight mut be greater that zero."
                });
            }

            string[] allowedStatuses =
            {
                "Pending","PickedUp","InTransit","Delivered","Cancelled"
            };

            if (string.IsNullOrWhiteSpace(shipment.Status) ||
                !allowedStatuses.Contains(shipment.Status))
            {
                return BadRequest(new
                {
                    message = "Invalid shipment status."
                });
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            string query = @"UPDATE Shipments SET 
                             SenderName = @SenderName,
                             ReceiverName = @ReceiverName,
                             ReceiverPhone = @ReceiverPhone,
                             PickupAddress = @PickupAddress,
                             DeliveryAddress = @DeliveryAddress,
                             Weight = @Weight,
                             Status = @Status
                             WHERE ShipmentId = @ShipmentId";

            //Staff can update only their own shipment
            if(roleId == 2)
            {
                query += " AND UserId = @UserId";
            }

            using (SqlConnection con  = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@SenderName", shipment.SenderName);
                    cmd.Parameters.AddWithValue("@ReceiverName", shipment.ReceiverName);
                    cmd.Parameters.AddWithValue("@ReceiverPhone", shipment.ReceiverPhone);
                    cmd.Parameters.AddWithValue("@PickupAddress", shipment.PickupAddress);
                    cmd.Parameters.AddWithValue("@DeliveryAddress", shipment.DeliveryAddress);
                    cmd.Parameters.AddWithValue("@Weight", shipment.Weight);
                    cmd.Parameters.AddWithValue("@Status", shipment.Status);
                    cmd.Parameters.AddWithValue("@ShipmentId", id);

                    if (roleId == 2)
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                    }

                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if(rowsAffected == 0)
                    {
                        return NotFound(new
                        {
                            message = "Shipment not found."
                        });
                    }
                }
            }



            return Ok(new
            {
                success = true,
                message = "Shipment updated successfully !"
            });
        }

        [HttpDelete("{id:int}")]
        public IActionResult DeleteShipment(int id)
        {
            if(!TryGetCurrentUser(out int userId, out int roleId))
            {
                return Unauthorized(new { message = "Please login first." });
            }

            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            string query = @"DELETE FROM Shipments
                             WHERE ShipmentId = @ShipmentId";

            if (roleId == 2)
            {
                query += " AND UserId = @UserId";
            }

           using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ShipmentId", id);

                    if (roleId == 2)
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                    }

                    con.Open();

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new
                        {
                            message = "Shipment not found."
                        });
                    }
                }
            }
            return Ok(new
            {
                success = true,
                message = "Shipment deleted successfully !"
            });
        }
    }

}

