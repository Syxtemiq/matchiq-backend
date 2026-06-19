namespace MatchIQ.Application.Modules.Auth;

// Maneja registro, login, verificación de email, Google OAuth y refresh tokens
// Equivalente a auth.service.js del backend Node
public class AuthService
{
    // TODO: inyectar AppDbContext, IJwtService, IEmailService, IPasswordHasher

    // TODO: RegisterAsync(RegisterDto dto)
    //       - verifica que el email no exista
    //       - hashea la contraseña
    //       - crea User + perfil según rol (CandidateProfile o CompanyProfile)
    //       - envía código de verificación por email

    // TODO: VerifyEmailAsync(string email, string code)

    // TODO: LoginAsync(LoginDto dto)
    //       - valida credenciales
    //       - verifica que el email esté verificado
    //       - retorna AccessToken + RefreshToken

    // TODO: RefreshTokenAsync(string refreshToken)

    // TODO: HandleGoogleLoginAsync(GoogleUserInfo info)
    //       - candidatos y empresa pueden usar Google OAuth
    //       - si no existe, crea el usuario automáticamente

    // TODO: ForgotPasswordAsync(string email)
    // TODO: ResetPasswordAsync(string token, string newPassword)
}
