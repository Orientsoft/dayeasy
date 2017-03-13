
using DayEasy.Core;
using DayEasy.Utility;
using DayEasy.Web.Wiki.Models;

namespace DayEasy.Web.Wiki.Contracts
{
    public interface IUserContract : IDependency
    {
        string CreateUser(string account, string pwd, string name, int role);
        DResult<User> Login(string account, string pwd);
    }
}