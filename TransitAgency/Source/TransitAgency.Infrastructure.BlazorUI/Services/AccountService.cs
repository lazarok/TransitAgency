using TransitAgency.Infrastructure.BlazorUI.Models;
using TransitAgency.Infrastructure.BlazorUI.Models.Account;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace TransitAgency.Infrastructure.BlazorUI.Services
{
    public interface IAccountService
    {
        User User { get; }
        Task Initialize();
        Task Login(Login model);
        Task Logout();
        Task Register(AddUser model);
        Task<IList<User>> GetAll();
        Task<User> GetById(string id);
        Task Update(string id, EditUser model);
        Task Delete(string id);
    }

    public class AccountService : IAccountService
    {
        private IHttpService _httpService;
        private NavigationManager _navigationManager;
        private ILocalStorageService _localStorageService;
        private string _userKey = "user";

        public User User { get; private set; }

        public AccountService(
            IHttpService httpService,
            NavigationManager navigationManager,
            ILocalStorageService localStorageService
        )
        {
            _httpService = httpService;
            _navigationManager = navigationManager;
            _localStorageService = localStorageService;
        }

        public async Task Initialize()
        {
            User = await _localStorageService.GetItem<User>(_userKey);
        }

        public async Task Login(Login model)
        {
            var response = await _httpService.PostAsync<User>("account/authenticate", model);

            if (!response.Success)
                throw new Exception(response.StringErrors());
            User = response.Data;
            await _localStorageService.SetItem(_userKey, User);
        }

        public async Task Logout()
        {
            User = null;
            await _localStorageService.RemoveItem(_userKey);
            _navigationManager.NavigateTo("account/login");
        }

        public async Task Register(AddUser model)
        {
            var response = await _httpService.PostAsync("account/register", model);
            if (!response.Success)
                throw new Exception(response.StringErrors());
        }

        public async Task<IList<User>> GetAll()
        {
            return await Task.Run(() => new List<User>());
        }

        public async Task<User> GetById(string id)
        {
            return await Task.Run(() => new User());
        }

        public Task Update(string id, EditUser model)
        {
           return Task.Delay(10);
        }

        public Task Delete(string id)
        {
            return Task.Delay(10);
        }
    }
}