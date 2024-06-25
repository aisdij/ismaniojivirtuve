namespace Project.Backend.Server.AuthenticationService
{
    public interface IHashingService
    {
        public bool Verify(string hashedPassword, string password);

        public string Hash(string password);
    }
}
