using Amazoom.Data.Base;
using Amazoom.Data.ViewModels;
using Amazoom.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Amazoom.Data.Services
{
    public interface IItemsService:IEntityBaseRepository<Item>
    {
        Task<Item> GetItemByIdAsync(int id);
        Task AddNewItemAsync(NewItemVM data);
        Task UpdateItemAsync(NewItemVM data);
        Task UpdateStockAsync(int id, int soldNumber);

    }
}
