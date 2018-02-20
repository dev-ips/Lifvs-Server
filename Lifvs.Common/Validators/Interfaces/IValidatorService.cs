using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lifvs.Common.Validators.Interfaces
{
    public interface IValidatorService<in T>
    {
        void Validate(T input);
    }
}
