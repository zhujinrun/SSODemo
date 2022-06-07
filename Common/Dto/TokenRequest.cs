using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Dto
{
    public class TokenRequest
    {
        /// <summary>
        /// 授权code
        /// </summary>
        public string AuthCode { get; set; }
    }
}
