using Microsoft.Msagl.Core.Geometry.Curves;
using Partlyx.Core.VisualsInfo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Partlyx.ViewModels.Graph;

public class EdgeViewModel
{
    public EdgeViewModel() { }
    public EdgeViewModel(EdgeGeometryData geometry)
        => Geometry = geometry;
    public EdgeGeometryData? Geometry { get; set; }

    public Guid Uid { get; } = Guid.NewGuid();
}