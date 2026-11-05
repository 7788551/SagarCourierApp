using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SagarCourierApp.Models;

namespace SagarCourierApp.ApiController
{
    [Route("api/[controller]")]
    [ApiController]
    public class StaffApiController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        public StaffApiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }



        [HttpGet]
        public IActionResult GetStaff()
        {
            List<User> user = new List<User>();

            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            string query = @"SELECT UserId,FullName,Email,RoleId
                              FROM Users WHERE RoleId = 2";
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            user.Add(new User
                            {
                                UserId = Convert.ToInt32(reader["UserId"]),
                                FullName = reader["FullName"].ToString(),
                                Email = reader["Email"].ToString(),
                                RoleId = Convert.ToInt32(reader["RoleId"])
                            });
                        }
                    }
                }
            }
            return Ok(user);
        }

        [HttpGet("{id}")]
        public IActionResult GetUserById(int id)
        {
            User user = null;
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            string query = @"SELECT UserId,FullName,Email,RoleId
                             FROM Users WHERE UserId = @UserId";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", id);
                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            user = new User
                            {
                                UserId = Convert.ToInt32(dr["UserId"]),
                                FullName = dr["FullName"].ToString(),
                                Email = dr["Email"].ToString(),
                                RoleId = Convert.ToInt32(dr["RoleId"])
                            };

                            
                        };
                    }
                }
            }
            if (user == null)
            {
                return NotFound("User not Found");
            }
            return Ok(user);
        }



        [HttpPut("{id}")]
        public IActionResult UpdateStaff(int id,[FromBody]User user)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            string query = @"UPDATE Users SET 
                              FullName = @FullName,
                              Email = @Email
                              WHERE UserId = @UserId";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@FullName", user.FullName);
                    cmd.Parameters.AddWithValue("@Email",user.Email);
                    cmd.Parameters.AddWithValue("@UserId",id);

                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if(rowsAffected == 0)
                    {
                        return NotFound(new
                        {
                          message = "Product not Found"
                        });
                    }
                }
            }
            return Ok(new
            {
                message = "Product Updated Successfully"
            });
        }


        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            // Check whether user is logged in
            string userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    success = false,
                    message = "Please login first!"
                });
            }

            // Check whether logged-in user is Admin
            string roleId = HttpContext.Session.GetString("RoleId");

            if (roleId != "1")
            {
                return StatusCode(403, new
                {
                    success = false,
                    message = "Access denied! Only Admin can delete staff."
                });
            }

            string connectionString =
                _configuration.GetConnectionString("DefaultConnection");

            string query = @"DELETE FROM Users
                     WHERE UserId = @UserId
                     AND RoleId = 2";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", id);

                    con.Open();

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        return NotFound(new
                        {
                            message = "Staff not found!"
                        });
                    }
                }
            }

            return Ok(new
            {
                success = true,
                message = "Staff deleted successfully!"
            });
        }

    }
}
