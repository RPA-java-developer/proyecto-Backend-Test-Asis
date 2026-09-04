using System;
using System.Collections.Generic;
using System.Text;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Xunit;
using ASISYA.Models;
using ASISYA.Services;



namespace ASISYA.Tests
{
    public class JwtServiceTests
    {
        // Configuración de prueba en memoria (no depende de appsettings.json real)
        private static IConfiguration BuildTestConfig()
        {
            var settings = new Dictionary<string, string?>
            {
                ["Jwt:Key"] = "clave-de-prueba-solo-para-tests-no-usar-en-produccion-123456",
                ["Jwt:Issuer"] = "ASISYA-Tests",
                ["Jwt:Audience"] = "ASISYA-Tests-Client",
                ["Jwt:ExpiryMinutes"] = "60"
            };

            return new ConfigurationBuilder()
                .AddInMemoryCollection(settings)
                .Build();
        }

        [Fact]
        public void GenerateToken_DevuelveUnTokenNoVacio()
        {
            var jwtService = new JwtService(BuildTestConfig());
            var user = new User { UserID = 1, Username = "admin" };

            var (token, _) = jwtService.GenerateToken(user);

            Assert.False(string.IsNullOrWhiteSpace(token));
        }

        [Fact]
        public void GenerateToken_LaFechaDeExpiracionEsFutura()
        {
            var jwtService = new JwtService(BuildTestConfig());
            var user = new User { UserID = 1, Username = "admin" };

            var (_, expiresAt) = jwtService.GenerateToken(user);

            Assert.True(expiresAt > DateTime.UtcNow);
        }

        [Fact]
        public void GenerateToken_ExpiraAproximadamenteEnLosMinutosConfigurados()
        {
            var jwtService = new JwtService(BuildTestConfig());
            var user = new User { UserID = 1, Username = "admin" };

            var before = DateTime.UtcNow;
            var (_, expiresAt) = jwtService.GenerateToken(user);

            // ExpiryMinutes = 60 en la config de prueba; damos 5s de margen
            // por el tiempo que toma ejecutar el propio test.
            var expectedExpiry = before.AddMinutes(60);
            Assert.True(Math.Abs((expiresAt - expectedExpiry).TotalSeconds) < 5);
        }

        [Fact]
        public void GenerateToken_IncluyeElUsernameComoClaim()
        {
            var jwtService = new JwtService(BuildTestConfig());
            var user = new User { UserID = 42, Username = "carlos" };

            var (token, _) = jwtService.GenerateToken(user);

            var handler = new JwtSecurityTokenHandler();
            var decoded = handler.ReadJwtToken(token);

            // Nota: como el token se crea con el constructor de JwtSecurityToken
            // directamente (no con CreateJwtSecurityToken), NO se aplica el
            // mapeo automático a nombres cortos de claims — el tipo se
            // conserva como la URI completa de ClaimTypes.Name.
            Assert.Contains(decoded.Claims, c => c.Type == ClaimTypes.Name && c.Value == "carlos");
        }

        [Fact]
        public void GenerateToken_IncluyeElIssuerYAudienceConfigurados()
        {
            var jwtService = new JwtService(BuildTestConfig());
            var user = new User { UserID = 1, Username = "admin" };

            var (token, _) = jwtService.GenerateToken(user);

            var handler = new JwtSecurityTokenHandler();
            var decoded = handler.ReadJwtToken(token);

            Assert.Equal("ASISYA-Tests", decoded.Issuer);
            Assert.Contains("ASISYA-Tests-Client", decoded.Audiences);
        }
    }
}
