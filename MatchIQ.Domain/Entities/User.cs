namespace MatchIQ.Domain.Entities;

// Representa un usuario del sistema (admin, candidato o empresa)
// Todo usuario tiene exactamente un perfil según su rol
public class User
{
    // TODO: Id, Email, PasswordHash, Role, IsActive, CreatedAt
    // TODO: relación 1-1 con CandidateProfile o CompanyProfile según el rol
    // TODO: relación 1-N con RefreshToken
}
