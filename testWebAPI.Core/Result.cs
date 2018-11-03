using System.Net;

namespace testWebAPI.Core
{
    public struct Result
    {
        public ResultCode Code { get; private set; }
        public string Message { get; private set; }

        public Result(ResultCode code, string message)
        {
            Code = code;
            Message = message;
        }

        public static Result None = new Result(ResultCode.None, "");
        public static Result Success(string message = "") => new Result(ResultCode.Success, message);
        public static Result InvalidArgument = new Result(ResultCode.InvalidArgument, "");
        public static Result Error(string message = "") => new Result(ResultCode.Error, message);
        public static Result Error(HttpStatusCode statusCode) => new Result(ResultCode.Error, statusCode.ToString());

        public enum ResultCode
        {
            None,
            Success,
            InvalidArgument,
            Error
        }

        public override string ToString()
        {
            return $"{Code} :\n{(string.IsNullOrEmpty(Message) ? "empty" : Message)}";
        }
    }
}
