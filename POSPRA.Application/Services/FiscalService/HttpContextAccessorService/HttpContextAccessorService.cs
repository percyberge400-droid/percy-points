using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POSPRA.Application.Services.FiscalService.IHttpContextAccessorService
{
    public class HttpContextAccessorService: IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }
    }
}
