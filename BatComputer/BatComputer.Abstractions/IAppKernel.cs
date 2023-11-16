using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MattEland.BatComputer.Abstractions.Widgets;

namespace MattEland.BatComputer.Abstractions;

public interface IAppKernel
{
    Queue<IWidget> Widgets { get; }
    void AddWidget(IWidget widget);
}