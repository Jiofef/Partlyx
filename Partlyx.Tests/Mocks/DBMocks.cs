using Microsoft.EntityFrameworkCore;
using Partlyx.Infrastructure.Data;
using Partlyx.Infrastructure.Data.Implementations;
using Partlyx.Infrastructure.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.Tests.Mocks
{
    public class TestPartlyxDBProvider : PartlyxDBProvider
    {
        public TestPartlyxDBProvider(IEventBus bus, IServiceProvider prov) : base(bus, prov) { }

        new public string? ConnectionString => $"DataSource=:memory:";
    }

    public class TestSettingsDBProvider : SettingsDBProvider
    {
        public TestSettingsDBProvider(IEventBus bus, IServiceProvider prov) : base(bus, prov) { }

        new public string? ConnectionString => $"DataSource=:memory:";
    }
}
