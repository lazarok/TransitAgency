using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TransitAgency.Infrastructure.BlazorUI.Models
{
    public class ApiResponse
    {
        public bool Success => Errors == null || Errors.Count == 0;

        public List<string> Errors { get; set; } = new List<string>();

        public void AddError(Exception exception)
        {
            AddError("Unhandled", exception.Message);
        }

        public void AddError(string status, string message)
        {
            Errors.Add($"{status} - {message}");
        }

        public string StringErrors()
        {
            if(Errors.Count == 0) return "";
            var str = "";
            foreach (var error in Errors)
                str += error + ". ";
            return str;
        }
    }

    public class ApiResponse<T> : ApiResponse
    {
        public T Data { get; set; }

    }
}
