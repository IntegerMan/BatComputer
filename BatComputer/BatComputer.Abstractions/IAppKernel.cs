using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MattEland.BatComputer.Abstractions;

public interface IAppKernel
{
    Queue<IWidget> Widgets { get; }
    void AddWidget(IWidget widget);
}