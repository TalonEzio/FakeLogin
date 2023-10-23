using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Data.SqlClient;

namespace FakeLogin.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : Controller
    {
        private readonly string? _connectionString;
        public AuthController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("FakeLogin");
        }
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            await using SqlConnection conn = new SqlConnection(_connectionString);
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand()
                {
                    CommandText = "usp_Login",
                    CommandType = CommandType.StoredProcedure,
                    Connection = conn
                };

                cmd.Parameters.AddWithValue("username", username);
                cmd.Parameters.AddWithValue("password", password);

                bool result = (int)(await cmd.ExecuteScalarAsync() ?? -1) > 0;
                if (result)
                {
                    return Ok(new
                    {
                        message = "Đăng nhập thành công",
                        loginStatus = result
                    });
                }
                return Unauthorized(new
                {
                    message = "Đăng nhập thất bại",
                    loginStatus = result
                });
            }
            return Unauthorized();
        }

        [HttpPut]
        public async Task<IActionResult> CreateUser([FromBody] UserViewModel model)
        {
            await using SqlConnection conn = new SqlConnection(_connectionString);
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand()
                {
                    CommandText = "usp_CreateUser",
                    CommandType = CommandType.StoredProcedure,
                    Connection = conn
                };

                cmd.Parameters.AddWithValue("username", model.UserName);
                cmd.Parameters.AddWithValue("password", model.Password);

                bool result = await cmd.ExecuteNonQueryAsync() > 0;
                if (result)
                {
                    return Ok(new
                    {
                        message = "Tạo tài khoản thành công",
                        loginStatus = result
                    });
                }
                return Unauthorized(new
                {
                    message = "Tạo tài khoản thất bại",
                    loginStatus = result
                });
            }
            return Unauthorized();
        }


        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            await using SqlConnection conn = new SqlConnection(_connectionString);
            if (conn.State != ConnectionState.Open)
            {
                await conn.OpenAsync();
                SqlCommand cmd = new SqlCommand()
                {
                    CommandText = "usp_GetAllUsers",
                    CommandType = CommandType.StoredProcedure,
                    Connection = conn
                };
                SqlDataReader userDataTable = await cmd.ExecuteReaderAsync();

                var userViewModelList = new List<UserViewModel>();

                if (userDataTable != null)
                {
                    while (userDataTable.Read())
                    {
                        var user = new UserViewModel()
                        {
                            Id = userDataTable.GetInt32(0),
                            UserName = userDataTable[1].ToString() ?? "",
                            Password = userDataTable[2].ToString() ?? ""
                        };
                        userViewModelList.Add(user);
                    }
                }
                return Ok(userViewModelList);
            }

            return NotFound();
        }
    }

    public class UserViewModel
    {
        public int Id { get; set; }
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }
}
