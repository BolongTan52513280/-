using Amazoom.Data.Base;
using Amazoom.Data.ViewModels;
using Amazoom.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Amazoom.Data.Services
{
    public class ItemsService : EntityBaseRepository<Item>, IItemsService
    {
        private readonly AppDbContext _context;
        public ItemsService(AppDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task AddNewItemAsync(NewItemVM data)
        {
            var newItem = new Item()
            {
                Name = data.Name,
                Description = data.Description,
                Price = data.Price,
                ImageURL = data.ImageURL,
                weight = data.weight,
                stock = data.stock,
                volume = data.volume,
            };
            await _context.Items.AddAsync(newItem);
            await _context.SaveChangesAsync();
           
        }

        public async Task<Item> GetItemByIdAsync(int id)
        {
            var ItemDetails = await _context.Items
                .FirstOrDefaultAsync(n => n.Id == id);

            return ItemDetails;
        }


        public async Task UpdateItemAsync(NewItemVM data)
        {
            var dbItem = await _context.Items.FirstOrDefaultAsync(n => n.Id == data.Id);

            if (dbItem != null)
            {
                dbItem.Name = data.Name;
                dbItem.Description = data.Description;
                dbItem.Price = data.Price;
                dbItem.ImageURL = data.ImageURL;
                dbItem.volume = data.volume;
                dbItem.weight = data.weight;
                dbItem.stock = data.stock;
                await _context.SaveChangesAsync();
            }

            await _context.SaveChangesAsync();
        }

        public async Task UpdateStockAsync(int id, int soldNumber)
        {
            var dbItem = await _context.Items.FirstOrDefaultAsync(n => n.Id == id);

            if (dbItem != null && dbItem.stock - soldNumber >= 0)
            {
                dbItem.stock = dbItem.stock - soldNumber;
                await _context.SaveChangesAsync();
            }
            await _context.SaveChangesAsync();
        }
    }
}
