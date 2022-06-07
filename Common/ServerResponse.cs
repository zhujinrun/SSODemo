using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public static class ServerResponseExtend
    {
        public static ServerResponse SetSuccess(this ServerResponse response, string message = "")
        {
            var result = new ServerResponse
            {
                Code = ResponseCode.Success,
                Message = message == string.Empty ? "操作成功" : message
            };
            return result;
        }

        public static ServerResponse<T> SetSuccess<T>(this ServerResponse<T> response, T data = default(T), string message = "")
        {
            response.Code = ResponseCode.Success;
            response.Message = message == string.Empty ? "操作成功" : message;
            response.Data = data;
            return response;
        }

        public static ServerResponse SetFail(this ServerResponse response, string message = "")
        {
            response.Code = ResponseCode.Fail;
            response.Message = message == string.Empty ? "操作失败" : message;
            return response;
        }

        public static ServerResponse<T> SetFail<T>(this ServerResponse<T> response, string message = "", T data = default(T))
        {
            response.Code = ResponseCode.Fail;
            response.Message = message == string.Empty ? "操作成功" : message;
            response.Data = data;
            return response;
        }
    }

    /// <summary>
    /// 响应结果
    /// </summary>
    public class ServerResponse
    {
        /// <summary>
        /// 响应状态码
        /// </summary>
        public ResponseCode Code { get; set; }

        /// <summary>
        /// 响应消息
        /// </summary>
        public string Message { get; set; }
    }

    /// <summary>
    /// 有对象的响应结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ServerResponse<T> : ServerResponse
    {
        public T Data { get; set; }
    }

    /// <summary>
    /// 响应状态码
    /// </summary>
    public enum ResponseCode
    {
        Success = 0,
        Fail = 1
    }
}
