namespace MoleImperator;

public class ImperatorCredentials
{
    public ImperatorCredentials(byte server, string login, string password)
    {
        Server = server;
        Login = login;
        Password = password;
    }

    public byte Server { get; }
    public string Login { get; }
    public string Password { get; }
}