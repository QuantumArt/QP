namespace Quantumart.QP8.Security.Ldap;

public class SignInResult
{
    public SignInResult(SignInStatus status)
    {
        Status = status;
    }

    public SignInStatus Status { get; }

    public bool Succeeded => Status == SignInStatus.Succeeded;

    public static readonly SignInResult NotInitialized = new(SignInStatus.NotInitialized);
    public static readonly SignInResult NotFound = new(SignInStatus.NotFound);
}
