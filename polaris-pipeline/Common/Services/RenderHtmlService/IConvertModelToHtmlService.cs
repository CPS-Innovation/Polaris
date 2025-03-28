﻿using System.IO;
using System.Threading.Tasks;

namespace Common.Services.RenderHtmlService
{
    public interface IConvertModelToHtmlService
    {
        public Task<Stream> ConvertAsync<T>(T data);
    }
}
