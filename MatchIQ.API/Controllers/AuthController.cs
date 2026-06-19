namespace MatchIQ.API.Controllers;

// [ApiController]
// [Route("api/auth")]
public class AuthController // : ControllerBase
{
    // TODO: inyectar AuthService

    // POST api/auth/register
    // TODO: RegisterAsync([FromBody] RegisterDto dto)

    // POST api/auth/verify-email
    // TODO: VerifyEmailAsync([FromBody] VerifyEmailDto dto)

    // POST api/auth/login
    // TODO: LoginAsync([FromBody] LoginDto dto)

    // POST api/auth/refresh
    // TODO: RefreshTokenAsync([FromBody] RefreshTokenDto dto)

    // POST api/auth/google
    // TODO: GoogleLoginAsync([FromBody] GoogleTokenDto dto)
    //       recibe el id_token de Google desde Flutter Web
    //       lo valida y retorna el JWT propio de la app

    // POST api/auth/forgot-password
    // TODO: ForgotPasswordAsync([FromBody] ForgotPasswordDto dto)

    // POST api/auth/reset-password
    // TODO: ResetPasswordAsync([FromBody] ResetPasswordDto dto)

    // POST api/auth/logout
    // [Authorize]
    // TODO: LogoutAsync() — revoca el refresh token
}
