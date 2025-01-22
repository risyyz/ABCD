using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABCD.Services.Security {
    public interface ITokenService {
        Task<string> GenerateToken();
    }
}
