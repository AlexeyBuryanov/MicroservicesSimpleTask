﻿using System.Threading.Tasks;

namespace ManagementService.Services.Request
{
    public interface IRequestService
    {
        Task<TResult> GetAsync<TResult>(string uri);

        Task<TResult> PostAsync<TResult>(string uri);

        Task<TResult> PostAsync<TResult>(string uri, TResult data);

        Task<TResult> PostAsync<TRequest, TResult>(string uri, TRequest data);

        Task DeleteAsync(string uri);
    }
}