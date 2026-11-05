
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SagarCourierApp.Models;

namespace SagarCourierApp.ApiController
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountApiController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AccountApiController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // REGISTER

        [HttpPost("register")]
        public IActionResult Register(User obj)
        {
            if (string.IsNullOrWhiteSpace(obj.FullName) ||
                string.IsNullOrWhiteSpace(obj.Email) ||
                string.IsNullOrWhiteSpace(obj.Password))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "All fields are required!"
                });
            }

            string connectionString =
                _configuration.GetConnectionString("DefaultConnection")!;

            string query = @"INSERT INTO Users
                     (FullName, Email, PasswordHash, RoleId)
                     VALUES
                     (@FullName, @Email, @PasswordHash, @RoleId)";

            // Hash the password received from the registration form
            string passwordHash =
                BCrypt.Net.BCrypt.HashPassword(obj.Password);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string checkQuery =
                    "SELECT COUNT(1) FROM Users WHERE Email = @Email";

                using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@Email", obj.Email);

                    con.Open();

                    int count = Convert.ToInt32(checkCmd.ExecuteScalar());

                    if (count > 0)
                    {
                        return Conflict(new
                        {
                            success = false,
                            message = "Email already registered!"
                        });
                    }
                }

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@FullName", obj.FullName);
                    cmd.Parameters.AddWithValue("@Email", obj.Email);
                    cmd.Parameters.AddWithValue("@PasswordHash", passwordHash);
                    cmd.Parameters.AddWithValue("@RoleId", 2);

                    cmd.ExecuteNonQuery();
                }
            }

            return Ok(new
            {
                success = true,
                message = "User registration successful!"
            });
        }

        // LOGIN

        [HttpPost("login")]
        public IActionResult Login([FromBody] User obj)
        {
            if (string.IsNullOrWhiteSpace(obj.Email) ||
                string.IsNullOrEmpty(obj.Password))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Email and password are required!"
                });
            }

            string connectionString =
                _configuration.GetConnectionString("DefaultConnection")!;

            string query = @"SELECT UserId, FullName, Email,
                            PasswordHash, RoleId
                     FROM Users
                     WHERE Email = @Email";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Email", obj.Email);

                    con.Open();

                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        if (dr.Read())
                        {
                            string passwordHash =
                                dr["PasswordHash"].ToString()!;

                            // Verify entered password against stored BCrypt hash
                            bool isPasswordValid =
                                BCrypt.Net.BCrypt.Verify(
                                    obj.Password, passwordHash);

                            if (isPasswordValid)
                            {
                                string userId = dr["UserId"].ToString()!;
                                string role = dr["RoleId"].ToString()!;
                                int roleId = Convert.ToInt32(role);

                                // Store login information in Session
                                HttpContext.Session.SetString("UserId", userId);
                                HttpContext.Session.SetString("RoleId", role);

                                string redirectUrl;

                                if (roleId == 1)
                                {
                                    redirectUrl = "/Dashboard/AdminDashboard";
                                }
                                else if (roleId == 2)
                                {
                                    redirectUrl = "/Dashboard/StaffDashboard";
                                }
                                else
                                {
                                    return Unauthorized(new
                                    {
                                        success = false,
                                        message = "Invalid user role."
                                    });
                                }

                                return Ok(new
                                {
                                    success = true,
                                    message = "Login successful!",
                                    redirectUrl = redirectUrl
                                });
                            }
                        }
                    }
                }
            }

            return Unauthorized(new
            {
                success = false,
                message = "Invalid email or password!"
            });
        }


        // LOGOUT
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();

            return Ok(new
            {
                success = true,
                message = "Logout successful!"
            });
        }
    }
}
