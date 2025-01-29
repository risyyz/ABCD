namespace ABCD.Server.RequestModels {
    public class RegisterRequestModel {
        public string Email { get; set; }
        public string Password { get; set; }
        public string ConfirmPassword { get; set; }
    }
}
