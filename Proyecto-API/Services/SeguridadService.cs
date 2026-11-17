using Microsoft.IdentityModel.Tokens;
using ProyectoFinal_API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ProyectoFinal_API.Services
{
    public class SeguridadService(IConfiguration _configuration) : ISeguridadService
    {
        private const int TamanoSalt = 16;
        private const int TamanoHash = 32;
        private const int Iteraciones = 100000;
        private const string AudienciaRecuperacion = "SuperTico_Recuperacion";

        /// <summary>
        /// Formato guardado: Base64(salt de 16 bytes + hash de 32 bytes), PBKDF2-SHA256 con 100000 iteraciones.
        /// </summary>
        public string GenerarHash(string contrasena)
        {
            var salt = RandomNumberGenerator.GetBytes(TamanoSalt);
            var hash = Rfc2898DeriveBytes.Pbkdf2(contrasena, salt, Iteraciones, HashAlgorithmName.SHA256, TamanoHash);

            return Convert.ToBase64String(salt.Concat(hash).ToArray());
        }

        public bool VerificarHash(string contrasena, string hashGuardado)
        {
            try
            {
                var datos = Convert.FromBase64String(hashGuardado);
                if (datos.Length != TamanoSalt + TamanoHash)
                    return false;

                var salt = datos.AsSpan(0, TamanoSalt).ToArray();
                var hashEsperado = datos.AsSpan(TamanoSalt, TamanoHash).ToArray();
                var hashCalculado = Rfc2898DeriveBytes.Pbkdf2(contrasena, salt, Iteraciones, HashAlgorithmName.SHA256, TamanoHash);

                return CryptographicOperations.FixedTimeEquals(hashCalculado, hashEsperado);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public (string Token, DateTime Expiracion) GenerarToken(UsuarioResponse usuario)
        {
            var minutos = _configuration.GetValue<int>("Jwt:MinutosExpiracion");
            var expiracion = DateTime.UtcNow.AddMinutes(minutos);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.UsuarioId.ToString()),
                new Claim(ClaimTypes.Name, usuario.NombreUsuario),
                new Claim(ClaimTypes.Email, usuario.Correo),
                new Claim(ClaimTypes.Role, usuario.NombreRol)
            };

            var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("Jwt:Clave")!));

            var token = new JwtSecurityToken(
                issuer: _configuration.GetValue<string>("Jwt:Emisor"),
                audience: _configuration.GetValue<string>("Jwt:Audiencia"),
                claims: claims,
                expires: expiracion,
                signingCredentials: new SigningCredentials(clave, SecurityAlgorithms.HmacSha256));

            return (new JwtSecurityTokenHandler().WriteToken(token), expiracion);
        }

        /// <summary>
        /// Usa una audiencia distinta a la del inicio de sesión, así este token no sirve
        /// para llamar endpoints protegidos y el token de sesión no sirve para restablecer contraseñas.
        /// </summary>
        public string GenerarTokenRecuperacion(int usuarioId)
        {
            var minutos = _configuration.GetValue<int>("Recuperacion:MinutosExpiracion");
            var clave = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("Jwt:Clave")!));

            var token = new JwtSecurityToken(
                issuer: _configuration.GetValue<string>("Jwt:Emisor"),
                audience: AudienciaRecuperacion,
                claims: [new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString())],
                expires: DateTime.UtcNow.AddMinutes(minutos),
                signingCredentials: new SigningCredentials(clave, SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public int? ValidarTokenRecuperacion(string token)
        {
            var parametros = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration.GetValue<string>("Jwt:Emisor"),
                ValidAudience = AudienciaRecuperacion,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetValue<string>("Jwt:Clave")!)),
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = new JwtSecurityTokenHandler().ValidateToken(token, parametros, out _);
                return int.TryParse(principal.FindFirstValue(ClaimTypes.NameIdentifier), out var usuarioId) ? usuarioId : null;
            }
            catch (Exception ex) when (ex is SecurityTokenException or ArgumentException)
            {
                return null;
            }
        }
    }
}
