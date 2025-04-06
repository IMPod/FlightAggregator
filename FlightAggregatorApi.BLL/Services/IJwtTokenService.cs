﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FlightAggregatorApi.BLL.Services
{
    public interface IJwtTokenService
    {
        string GenerateJwtToken(string username);
    }
}
