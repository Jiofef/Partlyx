using Microsoft.Extensions.DependencyInjection;
using Partlyx.Services.Commands;
using Partlyx.Tests.DataTests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Tests.InfrastructureTests
{
    [Collection("InMemoryDB")]
    public class CommonRecipeComponentsCommandsTest 
    {
        private readonly ServiceProvider _provider;

        public CommonRecipeComponentsCommandsTest(DBFixture fixture)
        {
            _provider = fixture.CreateProvider(services =>
            {
            });
        }
        public void Dispose() => _provider.Dispose();
    }
}
