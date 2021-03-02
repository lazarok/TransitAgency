using System;
using System.Collections.Generic;
using System.Text;

namespace TransitAgency.Application.Interfaces
{
    public interface IAuthenticatedUserService
    {
        string UserId { get; }
    }
}
