namespace apiDealManagement.Model.Response
{
    public class AuthenticateResponse
    {
        public string Message { get; set; }
        public bool Success { get; set; }
        public string Token { get; set; }
        public string Email { get; set; }
        public int? ID { get; set; }
        public int? Is_Admin { get; set; }
        public string Name { get; set; }
        public AuthenticateResponse(string token, bool success, string message, string email, int? id, int? is_admin, string name)
        {
            Token = token;
            Success = success;
            Message = message;
            Email = email;
            ID = id;
            Is_Admin = is_admin;
            Name = name;
        }
    }
}
