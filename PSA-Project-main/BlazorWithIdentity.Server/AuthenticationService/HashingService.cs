using System.Security.Cryptography;

namespace Project.Backend.Server.AuthenticationService
{
    public class HashingService : IHashingService
    {
        private readonly int _saltSize = 128 / 8;
        private readonly int _keySize = 256 / 8;
        private readonly int _iterations = 10000;
        private readonly HashAlgorithmName r_hashAlgorithmName = HashAlgorithmName.SHA256;
        private readonly char r_delimeter = ';';


        public string Hash(string password)
        {
            var salt = RandomNumberGenerator.GetBytes(_saltSize);
            var hash = Rfc2898DeriveBytes.Pbkdf2(password, salt, _iterations, r_hashAlgorithmName, _keySize);

            return string.Join(r_delimeter, Convert.ToBase64String(salt), Convert.ToBase64String(hash));
        }

        public bool Verify(string hashedPassword, string password)
        {
            var elements = hashedPassword.Split(r_delimeter);
            var salt = Convert.FromBase64String(elements[0]);
            var hash = Convert.FromBase64String(elements[1]);

            var hashInput = Rfc2898DeriveBytes.Pbkdf2(password, salt, _iterations, r_hashAlgorithmName, _keySize);

            return CryptographicOperations.FixedTimeEquals(hash, hashInput);
        }
    }

}
