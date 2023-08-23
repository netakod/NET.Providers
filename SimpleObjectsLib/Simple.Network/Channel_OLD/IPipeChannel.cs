using System;
using System.IO.Pipelines;
using System.Threading.Tasks;

namespace Simple.Network
{
    public interface IPipeChannel
    {
        Pipe In { get; }

        Pipe Out { get; }

        IPipelineFilter PipelineFilter { get; }
    }
}
